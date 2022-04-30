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
        private readonly IUserRepo _userRepo;
        private readonly ITokenManager _tokenManager;
        private readonly IUserManager _userManager;

        public AccountController(IUserRepo userRepo, ITokenManager tokenManager, IUserManager userManager)
        {
            _userRepo = userRepo;
            _tokenManager = tokenManager;
            _userManager = userManager;
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

        
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto user)
        {
            try
            {
                var userInDb = await _userRepo.FindUserByUsername(user.Username);

                if (userInDb == null)
                {
                    return BadRequest("Username and/or password is incorrect");
                }

                var verifiedPassword = BCrypt.Net.BCrypt.Verify(user.Password, userInDb.Password);

                if (!verifiedPassword)
                {
                    return BadRequest("Username and/or password is incorrect");
                }

                var rolesList = await _userRepo.GetRoles(userInDb.Id);
                
                var token = await _tokenManager.GenerateJwtToken(userInDb);

                return Ok(new {token, roles = rolesList});
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }

        }

        
        [HttpPut("update-password")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> ChangePassword(UpdatePasswordDto updatePassword)
        {
            if (updatePassword.CurrentPassword == updatePassword.NewPassword)
            {
                return BadRequest("New password cannot be the same as the current password.");
            }

            
            try
            {
                var user = await _userRepo.FindUserById(UserId);

                var currentPasswordIsCorrect = BCrypt.Net.BCrypt.Verify(updatePassword.CurrentPassword, user.Password);

                if (!currentPasswordIsCorrect) return BadRequest("Current password is not correct");
                
                var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(updatePassword.NewPassword);
                
                await _userRepo.ChangePassword(user.Id, newHashedPassword);
                return Ok("Password changed.");
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }
    }
}