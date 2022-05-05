using BackendTest.Dtos;
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
        private readonly IMovieRepo _movieRepo;

        public MoviesController(IMovieRepo movieRepo)
        {
            _movieRepo = movieRepo;
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetMovies()
        {
            try
            {
                var movies = await _movieRepo.FindUserMovies(UserId);

                if (!movies.Any()) return NotFound("You have no movies");
                
                return Ok(movies);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }


        [HttpPost("add-movie")]
        public async Task<IActionResult> AddMovie(MovieDto movie)
        {
            try
            {
                var userMovies = await _movieRepo.FindUserMovies(UserId);

                var isMovieNameDuplicate = userMovies.Any(userMovieInDb => userMovieInDb.Name == movie.Name);
                
                if (isMovieNameDuplicate) return BadRequest("A movie with this name already exists.");

                await _movieRepo.InsertMovieIntoDb(UserId, movie);
                return Ok("Movie created successfully");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }



        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMovie(int id, MovieDto movie)
        {
            try
            {
                var movieInDb = await _movieRepo.FindMovieById(id);
        
                if (movieInDb == null) return NotFound("Movie not found");

                await _movieRepo.UpdateMovieInDb(movieInDb, movie);
                
                return Ok("Movie updated successfully");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }
        
        
        
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                var movieInDb = await _movieRepo.FindMovieById(id);

                if (movieInDb == null) return NotFound("Movie not found");

                await _movieRepo.DeleteMovieFromDb(UserId, id);

                return Ok("Movie deleted successfully");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }
    }
}
