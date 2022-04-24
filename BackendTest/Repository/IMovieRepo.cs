using BackendTest.Dtos;

namespace BackendTest.Repository;

public interface IMovieRepo
{
    Task InsertMovieIntoDb(int userId, MovieDto movie);
    Task<List<MovieDto>> FindUserMovies(int userId);
}