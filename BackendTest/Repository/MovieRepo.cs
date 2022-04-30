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

        int directorId;
        if (directorInDb != null)
        {
            directorId = directorInDb.Id;    
        }
        else
        {
            var insertedDirector = await InsertIntoDirectorsTable(movie.DirectorName, connection);
            directorId = insertedDirector.Id;
        }
        
        
        var insertedMovie = await InsertIntoMoviesTable(movie.Name, movie.Year, directorId, connection);
        
        await InsertIntoUserMoviesTable(userId, insertedMovie.Id, connection);
        
        var insertedActorsList = await InsertIntoActorsTable(movie.Actors, connection);
        
        await InsertIntoMovieActorsTable(insertedMovie.Id, insertedActorsList, connection);
    }
    
    public async Task<List<MovieInDbDto>> FindUserMovies(int userId)
    {
        var query =
            @"SELECT UserMovies.UserId, UserMovies.MovieId, Movies.Name, Movies.[Year], Directors.Name AS 'DirectorName', Actors.Name AS 'ActorName' FROM UserMovies 
            JOIN Movies ON UserMovies.MovieId = Movies.Id 
            JOIN Directors ON Directors.Id = Movies.DirectorId 
            JOIN MovieActors ON Movies.Id = MovieActors.MovieId
            JOIN Actors ON Actors.Id = MovieActors.ActorId WHERE UserId = @userId";



        var parameters = new DynamicParameters();
        parameters.Add("userId", userId, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var movies = await connection.QueryAsync<SingleRowMovie>(query, parameters);

        var mergedMovies = MergeActorNames(movies);

        return mergedMovies;
    }
    
    public async Task<MovieInDbDto> FindMovieById(int id)
    {
        var query = @"SELECT Movies.Id AS MovieId, Movies.Name, Movies.[Year], Directors.Name AS DirectorName, Actors.Name AS ActorName FROM Movies 
                      LEFT JOIN Directors ON Directors.Id = Movies.DirectorId
                      LEFT JOIN MovieActors ON Movies.Id = MovieActors.MovieId
                      LEFT JOIN Actors ON MovieActors.ActorId = Actors.Id WHERE Movies.Id = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", id, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var movies = await connection.QueryAsync<SingleRowMovie>(query, parameters);

        if (!movies.Any()) return null;
        
        var movie = MergeActorNames(movies)[0];

        return movie;
    }
    
    public async Task UpdateMovieInDb(MovieInDbDto movieInDb, MovieDto movieForUpdate)
    {
        using var connection = _dapperContext.CreateConnection();

        await UpdateMoviesAndDirectorsTables(movieInDb, movieForUpdate, connection);

        await UpdateActorsAndMovieActorsTables(movieInDb.Id, movieForUpdate, connection);
    }
    
    public async Task DeleteMovieFromDb(int userId, int movieId)
    {
        using var connection = _dapperContext.CreateConnection();

        await AdminDeleteFromUserMoviesTable(movieId, connection);

        await DeleteFromMoviesAndMovieActorsTables(movieId, connection);
    }
    
    public async Task<IEnumerable<MovieInDbDto>> FindMoviesByYearRange(int startYear, int endYear)
    {
        var query = @"SELECT  Movies.Id AS MovieId, UserMovies.UserId, Movies.Name, Year, Directors.Name AS DirectorName, Actors.Name AS ActorName FROM Movies 
                    LEFT JOIN Directors ON Movies.DirectorId = Directors.Id
                    LEFT JOIN MovieActors ON Movies.Id = MovieActors.MovieId
                    LEFT JOIN Actors ON Actors.Id = MovieActors.ActorId
                    LEFT JOIN UserMovies ON Movies.Id = UserMovies.MovieId
                    WHERE Year >= @startYear AND Year <= @endYear";

        var parameters = new DynamicParameters();
        parameters.Add("startYear", startYear, DbType.Int32);
        parameters.Add("endYear", endYear, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var moviesInDb = await connection.QueryAsync<SingleRowMovie>(query, parameters);

        var movies = MergeActorNames(moviesInDb);

        return movies;
    }
    
    public async Task<IEnumerable<MovieInDbDto>> FindMovies(SearchParamsDto searchParams)
    {
        var query = @"SELECT  Movies.Id AS MovieId, UserMovies.UserId, Movies.Name, Year, Directors.Name AS DirectorName, Actors.Name AS ActorName FROM Movies 
                    LEFT JOIN Directors ON Movies.DirectorId = Directors.Id
                    LEFT JOIN MovieActors ON Movies.Id = MovieActors.MovieId
                    LEFT JOIN Actors ON Actors.Id = MovieActors.ActorId
                    LEFT JOIN UserMovies ON Movies.Id = UserMovies.MovieId WHERE";

        var queryConditions = @"";

        var parameters = new DynamicParameters();

        if (searchParams.StartYear != null && searchParams.EndYear != null)
        {
            var yearRangeQuery = @" Year >= @startYear AND Year <= @endYear";
            queryConditions += yearRangeQuery;
            
            parameters.Add("startYear", searchParams.StartYear, DbType.Int32);
            parameters.Add("endYear", searchParams.EndYear, DbType.Int32);
        }

        if (searchParams.MovieName != null)
        {
            var movieNameQuery = string.IsNullOrWhiteSpace(queryConditions)
                ? @" Movies.Name = @movieName"
                : @" AND Movies.Name = @movieName";

            queryConditions += movieNameQuery;
            
            parameters.Add("movieName", searchParams.MovieName, DbType.String);
        }

        if (searchParams.DirectorName != null)
        {
            var directorNameQuery = string.IsNullOrWhiteSpace(queryConditions)
                ? @" Directors.Name = @directorName"
                : @" AND Directors.Name = @directorName";

            queryConditions += directorNameQuery;
            
            parameters.Add("directorName", searchParams.DirectorName);
        }

        if (searchParams.Actors.Any())
        {
            for (var i = 0; i < searchParams.Actors.Count; i++)
            {
                if (i == 0)
                { 
                    var actorsNameQuery = string.IsNullOrWhiteSpace(queryConditions)
                        ? @" Actors.Name = @actorName"
                        : @" AND Actors.Name = @actorName";

                    queryConditions += actorsNameQuery;
                    
                    parameters.Add("actorName", searchParams.Actors[i], DbType.String);
                }
                else 
                {
                    var actorsNameQuery = string.IsNullOrWhiteSpace(queryConditions)
                        ? $@" Actors.Name = @actorName{i}"
                        : $@" OR Actors.Name = @actorName{i}";

                    queryConditions += actorsNameQuery;
                    
                    parameters.Add($"actorName{i}", searchParams.Actors[i], DbType.String);
                }
            }
        }

        query += queryConditions;

        Console.WriteLine(query);
        
        using var connection = _dapperContext.CreateConnection();

        var rawMovies = await connection.QueryAsync<SingleRowMovie>(query, parameters);

        var movies = MergeActorNames(rawMovies);

        return movies;
    }
    
    private async Task<MovieTable> InsertIntoMoviesTable(string movieName, int movieYear, int directorId, IDbConnection connection)
    {
        var query = @"INSERT INTO Movies (Name, Year, DirectorId) VALUES (@MovieName, @MovieYear, @DirectorId)" + "SELECT CAST(SCOPE_IDENTITY() as int)";

        var parameters = new DynamicParameters();
        parameters.Add("MovieName", movieName, DbType.String);
        parameters.Add("MovieYear", movieYear, DbType.Int32);
        parameters.Add("DirectorId", directorId, DbType.Int32);

        var createdMovieId = await connection.QuerySingleAsync<int>(query, parameters);

        var createdMovie = new MovieTable()
        {
            Id = createdMovieId,
            Name = movieName,
            Year = movieYear,
            DirectorId = directorId
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
    
    private async Task InsertIntoMovieActorsTable(int movieId, List<Actor> actors, IDbConnection connection)
    {
        var query = @"INSERT INTO MovieActors (MovieId, ActorId) VALUES (@movieId, @actorId)";

        var parameters = new DynamicParameters();

        foreach (var actor in actors)
        {
            parameters.Add("movieId", movieId, DbType.Int32);
            parameters.Add("actorId", actor.Id, DbType.Int32);

            await connection.ExecuteAsync(query, parameters);
        }
    }
    
    private async Task<Director> InsertIntoDirectorsTable(string directorName, IDbConnection connection)
    {
        var query = @"INSERT INTO Directors (Name) VALUES (@directorName)" + "SELECT CAST(SCOPE_IDENTITY() as int)";

        var param = new DynamicParameters();
        param.Add("directorName", directorName, DbType.String);

        var insertedDirectorId = await connection.QuerySingleAsync<int>(query, param);

        var insertedDirector = new Director
        {
            Id = insertedDirectorId,
            Name = directorName
        };

        return insertedDirector;
    }

    private async Task<List<Actor>> FindActors(List<string> actors, IDbConnection connection)
    {
        var query = @"SELECT * FROM Actors WHERE Name = @actorName";

        var actorsInDbList = new List<Actor>();

        foreach (var actorName in actors)
        {
            var parameters = new DynamicParameters();
            parameters.Add("actorName", actorName, DbType.String);
            
            var actorInDb = await connection.QueryFirstOrDefaultAsync<Actor>(query, parameters);

            if (actorInDb != null)
            {
                actorsInDbList.Add(actorInDb);
            }
        }

        return actorsInDbList;
    }
    
    private async Task<List<Actor>> InsertIntoActorsTable(List<string> actors, IDbConnection connection)
    {
        var actorsInDb = await FindActors(actors, connection);

        var actorsList = new List<Actor>();

        foreach (var actorObj in actorsInDb)
        {
            if (!actors.Contains(actorObj.Name)) continue;
            actorsList.Add(actorObj);
            actors.Remove(actorObj.Name);
        }

        foreach (var actorName in actors)
        {
            var query = @"INSERT INTO Actors (Name) VALUES (@actorName)" + "SELECT CAST(SCOPE_IDENTITY() as int)";
            
            var parameters = new DynamicParameters();
            parameters.Add("actorName", actorName, DbType.String);

            var newActorId = await connection.QuerySingleAsync<int>(query, parameters);

            var newActor = new Actor
            {
                Id = newActorId,
                Name = actorName
            };
            
            actorsList.Add(newActor);
        }

        return actorsList;
    }
    
    private async Task UpdateMoviesAndDirectorsTables(MovieInDbDto movieInDb, MovieDto movie, IDbConnection connection)
    {
        var directorInDb = await FindDirectorByName(movie.DirectorName, connection);

        int directorId;
        if (directorInDb != null)
        {
            directorId = directorInDb.Id;
        }
        else
        {
            var insertedDirector = await InsertIntoDirectorsTable(movie.DirectorName, connection);
            directorId = insertedDirector.Id;
        }
        
        

        var moviesTableHasChanges = movie.Name != movieInDb.Name || movie.Year != movieInDb.Year ||
                                   movie.DirectorName != directorInDb.Name;

        if (moviesTableHasChanges)
        {
            var query = @"UPDATE Movies 
                          SET Name = @movieName, 
                              Year = @movieYear, 
                              DirectorId = @directorId WHERE Id = @movieId";

            var parameters = new DynamicParameters();
            parameters.Add("movieName", movie.Name != movieInDb.Name ? movie.Name : movieInDb.Name, DbType.String);
            parameters.Add("movieYear", movie.Year != movieInDb.Year ? movie.Year : movieInDb.Year, DbType.Int32);
            parameters.Add("directorId", directorId, DbType.Int32);
            parameters.Add("movieId", movieInDb.Id, DbType.Int32);

            await connection.ExecuteAsync(query, parameters);

        }
    }
    
    private async Task UpdateActorsAndMovieActorsTables(int movieId, MovieDto movie, IDbConnection connection)
    {
        var newActors = await InsertIntoActorsTable(movie.Actors, connection);

        await DeleteMovieActorsByMovieId(movieId, connection);

        await InsertIntoMovieActorsTable(movieId, newActors, connection);

    }

    private async Task DeleteMovieActorsByMovieId(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM MovieActors WHERE MovieId = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }

    private List<string> GetActorsChanges(List<string> actorsInDb, List<string> actors)
    {
        foreach (var actorInDb in actorsInDb)
        {
            if (!actors.Contains(actorInDb)) continue;
            actors.Remove(actorInDb);
        }

        return actors;
    }

    private List<MovieInDbDto> MergeActorNames(IEnumerable<SingleRowMovie> movies)
    {
        var movieDictionary = new Dictionary<int, MovieInDbDto>();

        foreach (var movie in movies)
        {
            if (!movieDictionary.ContainsKey(movie.MovieId))
            {
                movieDictionary.Add(movie.MovieId, new MovieInDbDto
                {
                    UserId = movie.UserId,
                    Id = movie.MovieId,
                    Name = movie.Name,
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
    
    private async Task<Director> FindDirectorByName(string directorName, IDbConnection connection)
    {
        var query = @"SELECT * FROM Directors WHERE Name = @directorName";

        var parameters = new DynamicParameters();
        parameters.Add("directorName", directorName, DbType.String);

        var director = await connection.QueryFirstOrDefaultAsync<Director>(query, parameters);

        return director;
    }

    private async Task DeleteFromMoviesAndMovieActorsTables(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM MovieActors WHERE MovieId = @movieId;" +
                          @"DELETE FROM Movies WHERE Id = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }

    private async Task DeleteFromUserMoviesTable(int userId, int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM UserMovies WHERE MovieId = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }
    
    private async Task AdminDeleteFromUserMoviesTable(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM UserMovies WHERE MovieId = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }
}