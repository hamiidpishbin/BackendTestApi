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
        private readonly int _pageSize;

        public AdminController(IUserRepo userRepo, IMovieRepo movieRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _movieRepo = movieRepo;
            _pageSize = Convert.ToInt32(configuration["PaginationSize"]);
        }
        
        
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1)
        {
            try
            {
                var users = await _userRepo.FindAllUsers();

                if (!users.Any()) return BadRequest("No user is registered yet");

                var paginatedUserList = page > 1
                    ? users.Skip(_pageSize * page).Take(_pageSize)
                    : users.Take(_pageSize);
                
                return Ok(new {numberOfUsers = users.Count, numberOfPages = users.Count / _pageSize,  users = paginatedUserList});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }

        
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            try
            {
                var duplicateUser = await _userRepo.FindUserByUsername(user.Username);

                if (duplicateUser != null) return BadRequest("Username has already been taken");
                
                var createdUser = await _userRepo.CreateUser(user);

                return Ok(createdUser);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }

        
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepo.FindUserById(id);

                if (user == null) return NotFound("User not found!");

                await _userRepo.DeleteUser(user);

                return Ok("User deleted successfully.7y9");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }
        

        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto user)
        {
            try
            {
                var userIdDb = await _userRepo.FindUserById(id);

                if (userIdDb == null) return NotFound("User not found");

                await _userRepo.AdminUpdateUser(id, user);

                return Ok("User updated successfully");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }


        [HttpGet("movies")]
        public async Task<IActionResult> GetAllUsersAndMovies()
        {
            try
            {
                var users = await _userRepo.FindAllUsers();

                if (!users.Any()) return NotFound("No users found.");

                var userMoviesDictionary = new Dictionary<string, List<MovieInDbDto>>();
            
                foreach (var user in users)
                {
                    var userMovies = await _movieRepo.FindUserMovies(user.Id);
                    
                    if (userMovies.Any()) userMoviesDictionary.Add(user.Username, userMovies);
                }

                return Ok(userMoviesDictionary);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong");
            }
        }

        [HttpPut("movies/update/{id}")]
        public async Task<IActionResult> UpdateMovie([FromRoute]int id, [FromBody]MovieDto movie)
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


        [HttpDelete("movies/delete/{id}")]
        public async Task<IActionResult> DeleteMovie([FromRoute]int id)
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
        

        [HttpGet("movies/search")]
        public async Task<IActionResult> SearchMovies([FromBody] SearchParamsDto searchParams)
        {
            try
            {
                var movies = await _movieRepo.SearchMovies(searchParams);

                if (movies == null) return NotFound("Movie not found");

                return Ok(movies);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return exception is KeyNotFoundException 
                    ? BadRequest(exception.Message) 
                    : Problem("Something went wrong");
            }
        }
    }
}
