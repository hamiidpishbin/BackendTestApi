using System.Data;
using BackendTest.Data;
using BackendTest.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BackendTest.Repository;


public class MovieRepo : IMovieRepo
{
    private readonly DapperContext _dapperContext;

    public MovieRepo(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    
    
    public async Task InsertMovieIntoDb(int userId, MovieDto movie)
    {
        using var connection = _dapperContext.CreateConnection();

        var directorInDb = await FindDirectorByName(movie.DirectorName, connection);

        var directorId = directorInDb.Id;
        if (directorInDb == null)
        {
            var insertedDirector = await InsertIntoDirectorsTable(movie.DirectorName, connection);
            directorId = insertedDirector.Id;
        }
        
        
        var insertedMovie = await InsertIntoMoviesTable(movie.Name, movie.Year, directorId, connection);
        
        await InsertIntoUserMoviesTable(userId, insertedMovie.Id, connection);
        
        var insertedActorsList = await InsertIntoActorsTable(movie.Actors, connection);
        
        await InsertIntoMovieActorsTable(insertedMovie.Id, insertedActorsList, connection);
    }

    
    
    
    private async Task<MovieTableDto> InsertIntoMoviesTable(string movieName, int movieYear, int directorId, IDbConnection connection)
    {
        var query = @"INSERT INTO Movies (Name, Year, DirectorId) VALUES (@MovieName, @MovieYear, @DirectorId)" + "SELECT CAST(SCOPE_IDENTITY() as int)";

        var parameters = new DynamicParameters();
        parameters.Add("MovieName", movieName, DbType.String);
        parameters.Add("MovieYear", movieYear, DbType.Int32);
        parameters.Add("DirectorId", directorId, DbType.Int32);

        var createdMovieId = await connection.QuerySingleAsync<int>(query, parameters);

        var createdMovie = new MovieTableDto()
        {
            Id = createdMovieId,
            Name = movieName,
            Year = movieYear
        };

        return createdMovie;
    }




    private async Task InsertIntoUserMoviesTable(int userId, int movieId, IDbConnection connection)
    {
        var query = @"INSERT INTO UserMovies (UserId, MovieId) VALUES (@userId, @movieId)";

        var parameters = new DynamicParameters();
        parameters.Add("userId", userId, DbType.Int32);
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }

    private async Task InsertIntoMovieActorsTable(int movieId, List<ActorDto> actors, IDbConnection connection)
    {
        var query = @"INSERT INTO MovieActors (MovieId, ActorId) VALUES (@movieId, @actorId)";

        var parameters = new DynamicParameters();

        foreach (var actorObj in actors)
        {
            parameters.Add("movieId", movieId, DbType.Int32);
            parameters.Add("actorId", actorObj.Id, DbType.Int32);

            await connection.ExecuteAsync(query, parameters);
        }
    }

    
    
    
    
    private async Task<DirectorDto> InsertIntoDirectorsTable(string directorName, IDbConnection connection)
    {
        var query = @"INSERT INTO Directors (Name) VALUES (@directorName)" + "SELECT CAST(SCOPE_IDENTITY() as int)";

        var param = new DynamicParameters();
        param.Add("directorName", directorName, DbType.String);

        var insertedDirectorId = await connection.QuerySingleAsync<int>(query, param);

        var insertedDirector = new DirectorDto(directorName)
        {
            Id = insertedDirectorId
        };

        return insertedDirector;
    }




    private async Task<List<ActorDto>> InsertIntoActorsTable(List<string> actors, IDbConnection connection)
    {
        var query = @"INSERT INTO Actors (Name) VALUES (@actorName)" + "SELECT CAST(SCOPE_IDENTITY() as int)";

        var parameters = new DynamicParameters();
        var insertedActorsList = new List<ActorDto>();

        foreach (var actor in actors)
        {
            parameters.Add("actorName", actor, DbType.String);

            var insertedActorId = await connection.QuerySingleAsync<int>(query, parameters);

            var insertedActor = new ActorDto(actor)
            {
                Id = insertedActorId
            };
            
            insertedActorsList.Add(insertedActor);
        }

        return insertedActorsList;
    }

    public async Task<List<MovieDto>> FindUserMovies(int userId)
    {
        var query =
            @"SELECT UserMovies.MovieId, Movies.Name, Movies.[Year], Directors.Name AS 'DirectorName', Actors.Name AS 'ActorName' FROM UserMovies 
            JOIN Movies ON UserMovies.MovieId = Movies.Id 
            JOIN Directors ON Directors.Id = Movies.DirectorId 
            JOIN MovieActors ON Movies.Id = MovieActors.MovieId
            JOIN Actors ON Actors.Id = MovieActors.ActorId WHERE UserId = @userId";



        var parameters = new DynamicParameters();
        parameters.Add("userId", userId, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var movies = await connection.QueryAsync<UserMovieFromDbDto>(query, parameters);

        var mergedMovies = MergeActorNames(movies);

        return mergedMovies;
    }

    public async Task UpdateMovieInDb(int id, MovieDto movie)
    {
        using var connection = _dapperContext.CreateConnection();
        
        var movieIdDb = await FindMovieById(id);
        // if (movieIdDb == null)
        // {
        //     return null;
        // }

        await UpdateMovieTable(id, movieIdDb, movie, connection);
    }
    
    
    private async Task UpdateMovieTable(int movieId, MovieDto movieInDb, MovieDto movie, IDbConnection connection)
    {
        var directorInDb = await FindDirectorByName(movie.Name, connection);
        
        int DirectorId()
        {
            if (directorInDb == null)
            {
                var insertedDirector = InsertIntoDirectorsTable(movie.DirectorName, connection).Result;

                return insertedDirector.Id;
            }

            return directorInDb.Id;
        }

        if (movie.Name != movieInDb.Name || movie.Year != movieInDb.Year || movie.DirectorName != directorInDb.Name)
        {
            var query = @"UPDATE Movies 
                          SET Name = @movieName, 
                              Year = @movieYear, 
                              DirectorId = @directorId WHERE Id = @movieId";

            var parameters = new DynamicParameters();
            parameters.Add("movieName", movie.Name != movieInDb.Name ? movie.Name : movieInDb.Name, DbType.String);
            parameters.Add("movieYear", movie.Year != movieInDb.Year ? movie.Year : movieInDb.Year, DbType.Int32);
            parameters.Add("directorId", DirectorId(), DbType.Int32 );
            parameters.Add("movieId", movieId, DbType.Int32);

            await connection.ExecuteAsync(query, parameters);

        }
    }


    private List<MovieDto> MergeActorNames(IEnumerable<UserMovieFromDbDto> movies)
    {
        var movieDictionary = new Dictionary<int, MovieDto>();

        foreach (var movie in movies)
        {
            if (!movieDictionary.ContainsKey(movie.MovieId))
            {
                movieDictionary.Add(movie.MovieId, new MovieDto(movie.Name)
                {
                    Year = movie.Year,
                    DirectorName = movie.DirectorName,
                    Actors = new List<string>(){movie.ActorName}
                });
            }
            else
            {
                movieDictionary[movie.MovieId].Actors.Add(movie.ActorName);
            }
        }

        return movieDictionary.Values.ToList();
    }
    

    public async Task<MovieDto> FindMovieById(int id)
    {
        var query = @"SELECT Movies.Id AS MovieId, Movies.Name, Movies.[Year], Directors.Name AS DirectorName, Actors.Name AS ActorName FROM Movies 
                      JOIN Directors ON Directors.Id = Movies.DirectorId
                      JOIN MovieActors ON Movies.Id = MovieActors.MovieId
                      JOIN Actors ON MovieActors.ActorId = Actors.Id WHERE Movies.Id = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", id, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var movies = await connection.QueryAsync<UserMovieFromDbDto>(query, parameters);

        if (!movies.Any())
        {
            return null;
        }
        var movie = MergeActorNames(movies)[0];

        return movie;
    }



    private async Task<DirectorDto> FindDirectorByName(string directorName, IDbConnection connection)
    {
        var query = @"SELECT * FROM Directors WHERE Name = @directorName";

        var parameters = new DynamicParameters();
        parameters.Add("directorName", directorName, DbType.String);

        var director = await connection.QueryFirstOrDefaultAsync<DirectorDto>(query, parameters);

        return director;
    }
}