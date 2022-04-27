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
                return Ok(movies);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }


        [HttpPost("add-movie")]
        public async Task<IActionResult> AddMovie(MovieDto movie)
        {
            try
            {
                var userMovies = await _movieRepo.FindUserMovies(UserId);
                
                foreach (var movieObj in userMovies)
                {
                    if (movieObj.Name == movie.Name)
                    {
                        return BadRequest("You cannot insert a movie with duplicate name");
                    }
                }
                
                await _movieRepo.InsertMovieIntoDb(UserId, movie);
                return Ok();
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }



        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMovie(int id, MovieDto movie)
        {
            try
            {
                var movieInDb = await _movieRepo.FindMovieById(id);
        
                if (movieInDb == null)
                {
                    return NotFound("Movie not found");
                }
        
                await _movieRepo.UpdateMovieInDb(movieInDb, movie);
                return Ok();
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
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

                return Ok("Movie deleted from database");
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }
    }
}
