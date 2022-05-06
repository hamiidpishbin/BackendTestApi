using BackendTest.Dtos;
using BackendTest.Helpers;
using BackendTest.Models;
using BackendTest.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "USER")]
    public class MoviesController : BaseController
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMovieHelper _movieHelper;

        public MoviesController(IMovieRepository movieRepository, IMovieHelper movieHelper)
        {
            _movieRepository = movieRepository;
            _movieHelper = movieHelper;
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetMovies()
        {
            try
            {
                var movies = await _movieRepository.FindUserMovies(UserId);

                if (!movies.Any()) return NotFound(new ClientMessage{ErrorMessage = "No movies found"});
                
                return Ok(movies);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }


        [HttpPost("add-movie")]
        public async Task<IActionResult> AddMovie(MovieDto movie)
        {
            try
            {
                var userMovies = await _movieRepository.FindUserMovies(UserId);

                var isMovieNameDuplicate = userMovies.Any(userMovieInDb => userMovieInDb.Name == movie.Name);
                
                if (isMovieNameDuplicate) return BadRequest(new ClientMessage{ErrorMessage = "A movie with this name already exists."});

                await _movieRepository.InsertMovieIntoDb(UserId, movie);
                return Ok(new ClientMessage{SuccessMessage = "Movie created successfully"});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }



        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMovie(int id, MovieDto movie)
        {
            try
            {
                var rawMovieInDb = await _movieRepository.FindMovieById(id);
        
                if (!rawMovieInDb.Any()) return NotFound(new ClientMessage{ErrorMessage = "Movie not found"});

                var movieInDb = _movieHelper.MergeActorNames(rawMovieInDb).FirstOrDefault();

                await _movieRepository.UpdateMovieInDb(movieInDb, movie);
                
                return Ok(new ClientMessage{SuccessMessage = "Movie updated successfully"});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }
        
        
        
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                var movieInDb = await _movieRepository.FindMovieById(id);

                if (!movieInDb.Any()) return NotFound(new ClientMessage{ErrorMessage = "Movie not found"});

                await _movieRepository.DeleteMovieFromDb(UserId, id);

                return Ok(new ClientMessage{SuccessMessage = "Movie deleted successfully"});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }
    }
}
