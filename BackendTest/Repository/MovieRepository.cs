using System.Data;
using BackendTest.Data;
using BackendTest.Dtos;
using BackendTest.Services;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BackendTest.Repository;


public class MovieRepository : IMovieRepository
{
    private readonly DapperContext _dapperContext;
    private readonly ISearchParamsValidator _searchParamsValidator;

    public MovieRepository(DapperContext dapperContext, ISearchParamsValidator searchParamsValidator)
    {
        _dapperContext = dapperContext;
        _searchParamsValidator = searchParamsValidator;
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
        
        
        var insertedMovie = await InsertIntoMoviesTable(movie, directorId, connection);
        
        await InsertIntoUserMoviesTable(userId, insertedMovie.Id, connection);
        
        var insertedActorsList = await InsertIntoActorsTable(movie.Actors, connection);
        
        await InsertIntoMovieActorsTable(insertedMovie.Id, insertedActorsList, connection);
    }
    
    public async Task<IEnumerable<RawMovie>> FindUserMovies(int userId)
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

        var singleRowMovies = await connection.QueryAsync<RawMovie>(query, parameters);
        
        return singleRowMovies;
    }
    
    public async Task<IEnumerable<RawMovie>> FindMovieById(int id)
    {
        var query = @"SELECT Movies.Id AS MovieId, Movies.Name, Movies.[Year], Directors.Name AS DirectorName, Actors.Name AS ActorName FROM Movies 
                      LEFT JOIN Directors ON Directors.Id = Movies.DirectorId
                      LEFT JOIN MovieActors ON Movies.Id = MovieActors.MovieId
                      LEFT JOIN Actors ON MovieActors.ActorId = Actors.Id WHERE Movies.Id = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", id, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var movies = await connection.QueryAsync<RawMovie>(query, parameters);

        return movies;
    }
    
    public async Task UpdateMovieInDb(MovieInDbDto movieInDb, MovieDto movieForUpdate)
    {
        using var connection = _dapperContext.CreateConnection();

        var updatedDirector = await UpdateDirectorsTable(movieForUpdate.DirectorName, connection);

        await UpdateMoviesTable(movieInDb, movieForUpdate, updatedDirector, connection);

        var updatedActorsList = await UpdateActorsTable(movieInDb.Id, movieForUpdate, connection);

        await UpdateMovieActorsTable(movieInDb.Id, updatedActorsList, connection);
    }
    
    public async Task DeleteMovieFromDb(int userId, int movieId)
    {
        using var connection = _dapperContext.CreateConnection();

        await DeleteFromUserMoviesTable(movieId, connection);

        await DeleteFromMovieActorsTable(movieId, connection);

        await DeleteFromMoviesTable(movieId, connection);
    }
    
    public async Task<IEnumerable<RawMovie>> SearchMovies(SearchParamsDto searchParams)
    {
        var query = @"SELECT  Movies.Id AS MovieId, UserMovies.UserId, Movies.Name, Year, Directors.Name AS DirectorName, Actors.Name AS ActorName FROM Movies 
                    LEFT JOIN Directors ON Movies.DirectorId = Directors.Id
                    LEFT JOIN MovieActors ON Movies.Id = MovieActors.MovieId
                    LEFT JOIN Actors ON Actors.Id = MovieActors.ActorId
                    LEFT JOIN UserMovies ON Movies.Id = UserMovies.MovieId WHERE";

        var queryConditions = @"";

        var parameters = new DynamicParameters();
        
        if (_searchParamsValidator.IsActorsListValid(searchParams))
        {
            var movieIdList = await FindMovieIdByActorsList(searchParams.Actors);

            if (movieIdList.Any())
            {
                var actorsNameQuery = "";

                for (var i = 0; i < movieIdList.Count; i++)
                {
                    if (i == 0)
                    {
                        var subQuery = @"Movies.id = @movieId";

                        actorsNameQuery += subQuery;
                    
                        parameters.Add("movieId", movieIdList[i], DbType.String);
                    }
                    else
                    {
                        var subQuery = $@" OR Movies.Id = @movieId{i}";
                    
                        actorsNameQuery += subQuery;
                    
                        parameters.Add($"movieId{i}", movieIdList[i], DbType.String);
                    }
                }

                queryConditions += $" ({actorsNameQuery})";   
            }
        }

        if (_searchParamsValidator.IsYearRangeValid(searchParams))
        {
            var yearRangeQuery = string.IsNullOrWhiteSpace(queryConditions)
                ? @" (Year >= @startYear AND Year <= @endYear)"
                : @" AND (Year >= @startYear AND Year <= @endYear)";
            queryConditions += yearRangeQuery;
            
            parameters.Add("startYear", searchParams.StartYear, DbType.Int32);
            parameters.Add("endYear", searchParams.EndYear, DbType.Int32);
        }

        if (_searchParamsValidator.IsMovieNameValid(searchParams))
        {
            var movieNameQuery = string.IsNullOrWhiteSpace(queryConditions)
                ? @" Movies.Name = @movieName"
                : @" AND Movies.Name = @movieName";

            queryConditions += movieNameQuery;
            
            parameters.Add("movieName", searchParams.MovieName, DbType.String);
        }

        if (_searchParamsValidator.IsDirectorNameValid(searchParams))
        {
            var directorNameQuery = string.IsNullOrWhiteSpace(queryConditions)
                ? @" Directors.Name = @directorName"
                : @" AND Directors.Name = @directorName";

            queryConditions += directorNameQuery;
            
            parameters.Add("directorName", searchParams.DirectorName);
        }

        if (string.IsNullOrWhiteSpace(queryConditions)) throw new KeyNotFoundException("Movie not found");
        
        query += queryConditions;
        
        using var connection = _dapperContext.CreateConnection();

        var rawMovies = await connection.QueryAsync<RawMovie>(query, parameters);
        
        return rawMovies;
    }

    private async Task<List<int>> FindMovieIdByActorsList(List<string> actors)
    {
        var query =
            $@"SELECT Movies.Id, Movies.Name FROM Movies 
            LEFT JOIN MovieActors ON Movies.Id = MovieActors.MovieId 
            LEFT JOIN Actors ON MovieActors.ActorId = Actors.Id WHERE Actors.Name = @firstActor";

        var parameters = new DynamicParameters();
        parameters.Add("firstActor", actors[0], DbType.String);

        var queryConditions = @"";

        for (int i = 0; i < actors.Count; i++)
        {
            if (i > 0)
            {
                queryConditions += @" OR Actors.Name = @nextActor";
                parameters.Add("nextActor", actors[i], DbType.String);
            }
        }

        query += queryConditions;

        using var connection = _dapperContext.CreateConnection();

        var result = await connection.QueryAsync(query, parameters);
        
        var movieIdList = new List<int>();

        foreach (var resultObj in result)
        {
            if (!movieIdList.Contains(resultObj.Id))
            {
                movieIdList.Add(resultObj.Id);
            }
        }

        return movieIdList;
    }

    private async Task<MovieTable> InsertIntoMoviesTable(MovieDto movie, int directorId, IDbConnection connection)
    {
        var query = @"INSERT INTO Movies (Name, Year, DirectorId) VALUES (@MovieName, @MovieYear, @DirectorId)" + "SELECT CAST(SCOPE_IDENTITY() as int)";

        var parameters = new DynamicParameters();
        parameters.Add("MovieName", movie.Name, DbType.String);
        parameters.Add("MovieYear", movie.Year, DbType.Int32);
        parameters.Add("DirectorId", directorId, DbType.Int32);

        var createdMovieId = await connection.QuerySingleAsync<int>(query, parameters);

        var createdMovie = new MovieTable()
        {
            Id = createdMovieId,
            Name = movie.Name,
            Year = movie.Year,
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
    
    private async Task InsertIntoMovieActorsTable(int movieId, IEnumerable<Actor> actors, IDbConnection connection)
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

    private async Task<List<Actor>> FindActorsByNameList(List<string> actors, IDbConnection connection)
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
    
    private async Task<IEnumerable<Actor>> InsertIntoActorsTable(List<string> actors, IDbConnection connection)
    {
        var actorsInDb = await FindActorsByNameList(actors, connection);

        var actorsList = new List<Actor>();

        foreach (var actor in actorsInDb)
        {
            if (!actors.Contains(actor.Name)) continue;
            actorsList.Add(actor);
            actors.Remove(actor.Name);
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

    private async Task<Director> UpdateDirectorsTable(string directorName, IDbConnection connection)
    {
        var directorInDb = await FindDirectorByName(directorName, connection);

        if (directorInDb != null) return directorInDb;
        
        var insertedDirector = await InsertIntoDirectorsTable(directorName, connection);

        return insertedDirector;
    }
    
    private async Task UpdateMoviesTable(MovieInDbDto movieInDb, MovieDto movie, Director updatedDirector, IDbConnection connection)
    {
        var moviesTableHasChanges = movie.Name != movieInDb.Name || movie.Year != movieInDb.Year ||
                                    movie.DirectorName != movieInDb.DirectorName;

        if (moviesTableHasChanges)
        {
            var query = @"UPDATE Movies 
                          SET Name = @movieName, 
                              Year = @movieYear, 
                              DirectorId = @directorId WHERE Id = @movieId";

            var parameters = new DynamicParameters();
            parameters.Add("movieName", movie.Name != movieInDb.Name ? movie.Name : movieInDb.Name, DbType.String);
            parameters.Add("movieYear", movie.Year != movieInDb.Year ? movie.Year : movieInDb.Year, DbType.Int32);
            parameters.Add("directorId", updatedDirector.Id, DbType.Int32);
            parameters.Add("movieId", movieInDb.Id, DbType.Int32);

            await connection.ExecuteAsync(query, parameters);

        }
    }

    private async Task<IEnumerable<Actor>> UpdateActorsTable(int movieId, MovieDto movieForUpdate, IDbConnection connection)
    {
        var newActors = await InsertIntoActorsTable(movieForUpdate.Actors, connection);

        await DeleteMovieActorsByMovieId(movieId, connection);

        return newActors;
    }
    
    private async Task UpdateMovieActorsTable(int movieId, IEnumerable<Actor> newActors, IDbConnection connection)
    {
        await InsertIntoMovieActorsTable(movieId, newActors, connection);
    }

    private async Task DeleteMovieActorsByMovieId(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM MovieActors WHERE MovieId = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    } 

    private async Task<Director> FindDirectorByName(string directorName, IDbConnection connection)
    {
        var query = @"SELECT * FROM Directors WHERE Name = @directorName";

        var parameters = new DynamicParameters();
        parameters.Add("directorName", directorName, DbType.String);

        var director = await connection.QueryFirstOrDefaultAsync<Director>(query, parameters);

        return director;
    }

    private async Task DeleteFromMoviesTable(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM Movies WHERE Id = @movieId";
        
        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }
    
    private async Task DeleteFromMovieActorsTable(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM MovieActors WHERE MovieId = @movieId;";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }
    
    private async Task DeleteFromUserMoviesTable(int movieId, IDbConnection connection)
    {
        var query = @"DELETE FROM UserMovies WHERE MovieId = @movieId";

        var parameters = new DynamicParameters();
        parameters.Add("movieId", movieId, DbType.Int32);

        await connection.ExecuteAsync(query, parameters);
    }
}