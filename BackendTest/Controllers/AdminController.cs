using BackendTest.Dtos;
using BackendTest.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : BaseController
    {
        private readonly IUserRepo _userRepo;
        private readonly IMovieRepo _movieRepo;

        public AdminController(IUserRepo userRepo, IMovieRepo movieRepo)
        {
            _userRepo = userRepo;
            _movieRepo = movieRepo;
        }
        
        
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepo.FindAllUsers();

                if (!users.Any()) return BadRequest("No user is registered yet");

                return Ok(users);
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }

        
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            try
            {
                var duplicateUser = await _userRepo.FindUserByUsername(user.Username);

                if (duplicateUser != null)
                {
                    return BadRequest("Username is already taken");
                }
                
                var createdUser = await _userRepo.CreateUser(user);

                return Ok(createdUser);
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }

        
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepo.FindUserById(id);

                if (user == null)
                {
                    return NotFound("User not found!");
                }

                await _userRepo.DeleteUser(user);

                return Ok("User was deleted.");
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }
        

        [HttpPut("edit-user/{id}")]
        public async Task<IActionResult> EditUser(int id, UserDto user)
        {
            try
            {
                var userIdDb = await _userRepo.FindUserById(id);

                if (userIdDb == null)
                {
                    return NotFound("User not found");
                }

                await _userRepo.AdminEditUser(id, user);

                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpGet("movies")]
        public async Task<IActionResult> GetAllMovies()
        {
            try
            {
                var users = await _userRepo.FindAllUsers();

                if (!users.Any()) return NotFound("DB has no users.");

                var userMovieList = new List<MovieInDbDto>();
            
                foreach (var user in users)
                {
                    var userMovies = await _movieRepo.FindUserMovies(user.Id);

                    userMovieList.AddRange(userMovies);
                }

                return Ok(userMovieList);
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }

        [HttpPut("movies/update/{id}")]
        public async Task<IActionResult> EditMovie([FromRoute]int id, [FromBody]MovieDto movie)
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


        [HttpDelete("movies/delete/{id}")]
        public async Task<IActionResult> DeleteMovie([FromRoute]int id)
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
        

        [HttpGet("movies/search")]
        public async Task<IActionResult> SearchMovies([FromBody] SearchParamsDto searchParams)
        {
            try
            {
                var movies = await _movieRepo.FindMovies(searchParams);

                return Ok(movies);
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }
    }
}
