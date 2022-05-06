using BackendTest.Dtos;

namespace BackendTest.Repository;

public interface IMovieRepository
{
    Task InsertMovieIntoDb(int userId, MovieDto movie);
    Task<IEnumerable<SingleRowMovie>> FindUserMovies(int userId);
    Task UpdateMovieInDb(MovieInDbDto movieInDb, MovieDto movieForUpdate);
    Task<IEnumerable<SingleRowMovie>> FindMovieById(int movieId);
    Task DeleteMovieFromDb(int userId, int movieId);
    Task<IEnumerable<SingleRowMovie>> SearchMovies(SearchParamsDto searchParams);
}