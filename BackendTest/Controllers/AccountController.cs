using BackendTest.Dtos;
using BackendTest.Repository;
using BackendTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenManager _tokenManager;

        public AccountController(IUserRepository userRepository, ITokenManager tokenManager)
        {
            _userRepository = userRepository;
            _tokenManager = tokenManager;
        }
        
        
        
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] UserDto user)
        {
            try
            {
                if (user.Username == user.Password)
                {
                    return BadRequest("Username and password cannot be the same!");
                }
            
                var duplicateUser = await _userRepository.FindUserByUsername(user.Username);
                
                if (duplicateUser != null)
                {
                    return BadRequest("Username has already been taken.");
                }
                
                var createdUser = await _userRepository.CreateUser(user);

                await _userRepository.InsertIntoUserRolesTable(createdUser.Id);
                
                return Ok(createdUser);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return Problem("Something went wrong");
            }
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto user)
        {
            try
            {
                var userInDb = await _userRepository.FindUserByUsername(user.Username);

                if (userInDb == null)
                {
                    return BadRequest("Username or password is incorrect");
                }

                var verifiedPassword = BCrypt.Net.BCrypt.Verify(user.Password, userInDb.Password);

                if (!verifiedPassword)
                {
                    return BadRequest("Username or password is incorrect");
                }
                
                var token = await _tokenManager.GenerateJwtToken(userInDb);

                return Ok(new {message = "Logged in successfully", token});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return Problem("Something went wrong");
            }
        }

        
        [HttpPut("update-password")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> ChangePassword(UpdatePasswordDto updatePassword)
        {
            try
            {
                if (updatePassword.CurrentPassword == updatePassword.NewPassword)
                {
                    return BadRequest("New password cannot be the same as the current password.");
                }
                
                var user = await _userRepository.FindUserById(UserId);

                var currentPasswordIsCorrect = BCrypt.Net.BCrypt.Verify(updatePassword.CurrentPassword, user.Password);

                if (!currentPasswordIsCorrect) return BadRequest("Current password is not correct");
                
                var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(updatePassword.NewPassword);
                
                await _userRepository.ChangePassword(user.Id, newHashedPassword);
                
                return Ok(new {message = "Password changed successfully"});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return Problem("Something went wrong");
            }
        }
    }
}