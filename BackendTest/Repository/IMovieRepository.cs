using BackendTest.Dtos;

namespace BackendTest.Repository;

public interface IMovieRepository
{
    Task InsertMovieIntoDb(int userId, MovieDto movie);
    Task<IEnumerable<RawMovie>> FindUserMovies(int userId);
    Task UpdateMovieInDb(MovieInDbDto movieInDb, MovieDto movieForUpdate);
    Task<IEnumerable<RawMovie>> FindMovieById(int movieId);
    Task DeleteMovieFromDb(int userId, int movieId);
    Task<IEnumerable<RawMovie>> SearchMovies(SearchParamsDto searchParams);
}