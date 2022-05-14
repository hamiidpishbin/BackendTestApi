using BackendTest.Attributes;
using BackendTest.Dtos;
using BackendTest.Helpers;
using BackendTest.Models;
using BackendTest.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RoleAuthorization("ADMIN")]
    public class AdminController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IMovieHelper _movieHelper;
        private readonly int _pageSize;

        public AdminController(IUserRepository userRepository, IMovieRepository movieRepository, IConfiguration configuration, IMovieHelper movieHelper)
        {
            _userRepository = userRepository;
            _movieRepository = movieRepository;
            _movieHelper = movieHelper;
            _pageSize = Convert.ToInt32(configuration["PaginationSize"]);
        }
        
        
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1)
        {
            try
            {
                var users = await _userRepository.FindAllUsers();

                if (!users.Any()) return BadRequest(new ClientMessage{ErrorMessage = "No user is registered yet"});

                var paginatedUserList = page > 1
                    ? users.Skip(_pageSize * page).Take(_pageSize)
                    : users.Take(_pageSize);
                
                return Ok(new {numberOfUsers = users.Count, numberOfPages = users.Count / _pageSize,  users = paginatedUserList});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }

        
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            try
            {
                var duplicateUser = await _userRepository.FindUserByUsername(user.Username);

                if (duplicateUser != null) return BadRequest(new ClientMessage{ErrorMessage = "Username has already been taken"});
                
                var createdUser = await _userRepository.CreateUser(user);

                return Ok(createdUser);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }

        
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepository.FindUserById(id);

                if (user == null) return NotFound(new ClientMessage{ErrorMessage = "User not found!"});

                await _userRepository.DeleteUser(id);

                return Ok(new ClientMessage{SuccessMessage = "User deleted successfully."});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }
        

        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto user)
        {
            try
            {
                var userIdDb = await _userRepository.FindUserById(id);

                if (userIdDb == null) return NotFound(new ClientMessage{ErrorMessage = "User not found"});

                await _userRepository.AdminUpdateUser(id, user);

                return Ok(new ClientMessage{SuccessMessage = "User updated successfully"});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }


        [HttpGet("movies")]
        public async Task<IActionResult> GetAllUserMovies()
        {
            try
            {
                var users = await _userRepository.FindAllUsers();

                if (!users.Any()) return NotFound(new ClientMessage{ErrorMessage = "No users found."});

                var userMoviesList = new List<UserMoviesForAdminDto>();
            
                foreach (var user in users)
                {
                    var rawUserMovies = await _movieRepository.FindUserMovies(user.Id);

                    var userMovies = _movieHelper.MergeActorNames(rawUserMovies);

                    if (!userMovies.Any()) continue;
                    foreach (var movie in userMovies)
                    {
                        var userMovie = new UserMoviesForAdminDto
                        {
                            UserId = user.Id,
                            MovieId = movie.Id,
                            Name = movie.Name,
                            Year = movie.Year,
                            DirectorName = movie.DirectorName,
                            Actors = movie.Actors
                        };
                            
                        userMoviesList.Add(userMovie);
                    }
                }
                return Ok(new ClientMessage{SuccessMessage = "Movies list retrieved successfully", Data = userMoviesList});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return Problem("Something went wrong! Check logs for detail.");
            }
        }

        [HttpPut("movies/update/{id}")]
        public async Task<IActionResult> UpdateMovie([FromRoute]int id, [FromBody]MovieDto movie)
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


        [HttpDelete("movies/delete/{id}")]
        public async Task<IActionResult> DeleteMovie([FromRoute]int id)
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
        

        [HttpGet("movies/search")]
        public async Task<IActionResult> SearchMovies([FromBody] SearchParamsDto searchParams)
        {
            try
            {
                var rawMovies = await _movieRepository.SearchMovies(searchParams);

                if (!rawMovies.Any()) return NotFound(new ClientMessage{ErrorMessage = "Movie not found"});

                var movies = _movieHelper.MergeActorNames(rawMovies);

                return Ok(movies);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return exception is KeyNotFoundException 
                    ? BadRequest(exception.Message) 
                    : Problem("Something went wrong! Check logs for detail.");
            }
        }
    }
}
