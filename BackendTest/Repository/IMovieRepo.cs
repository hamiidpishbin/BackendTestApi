using BackendTest.Dtos;

namespace BackendTest.Repository;

public interface IMovieRepo
{
    Task InsertMovieIntoDb(int userId, MovieDto movie);
    Task<List<MovieInDbDto>> FindUserMovies(int userId);
    Task UpdateMovieInDb(MovieInDbDto movieInDb, MovieDto movieForUpdate);
    Task<MovieInDbDto> FindMovieById(int movieId);
    Task DeleteMovieFromDb(int userId, int movieId);
    Task<IEnumerable<MovieInDbDto>> FindMoviesByYearRange(int startYear, int endYear);
}