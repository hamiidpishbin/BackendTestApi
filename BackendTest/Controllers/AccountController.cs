using BackendTest.Dtos;
using BackendTest.Models;
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
                    return BadRequest(new ClientMessage{ErrorMessage = "Username and password cannot be the same!"});
                }
            
                var duplicateUser = await _userRepository.FindUserByUsername(user.Username);
                
                if (duplicateUser != null)
                {
                    return BadRequest(new ClientMessage{ErrorMessage = "Username has already been taken."});
                }
                
                var createdUser = await _userRepository.CreateUser(user);

                await _userRepository.InsertIntoUserRolesTable(createdUser.Id);
                
                return Ok(new ClientMessage
                {
                    SuccessMessage = "User was created successfully.",
                    Data = createdUser
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return Problem("Something went wrong! Check logs for detail.");
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
                    return BadRequest(new ClientMessage{ErrorMessage = "Username or password is incorrect"});
                }

                var verifiedPassword = BCrypt.Net.BCrypt.Verify(user.Password, userInDb.Password);

                if (!verifiedPassword)
                {
                    return BadRequest(new ClientMessage{ErrorMessage = "Username or password is incorrect"});
                }
                
                var token = await _tokenManager.GenerateJwtToken(userInDb);

                return Ok(new ClientMessage
                {
                    SuccessMessage = "Logged in successfully",
                    Data = token
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return Problem("Something went wrong! Check logs for detail.");
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
                    return BadRequest(new ClientMessage{ErrorMessage = "New password cannot be the same as the current password."});
                }
                
                var user = await _userRepository.FindUserById(UserId);

                var currentPasswordIsCorrect = BCrypt.Net.BCrypt.Verify(updatePassword.CurrentPassword, user.Password);

                if (!currentPasswordIsCorrect) return BadRequest(new ClientMessage{ErrorMessage = "Current password is not correct"});
                
                var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(updatePassword.NewPassword);
                
                await _userRepository.ChangePassword(user.Id, newHashedPassword);
                
                return Ok(new ClientMessage{SuccessMessage = "Password changed successfully"});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                
                return Problem("Something went wrong! Check logs for detail.");
            }
        }
    }
}