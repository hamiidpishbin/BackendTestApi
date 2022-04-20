using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendTest.Data;
using BackendTest.Dtos;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using BackendTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IUserRepo _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IUserRolesRepo _userRolesRepo;
        private readonly IUserManager _userManager;

        public AccountController(IUserRepo userRepo, ITokenService tokenService, IUserRolesRepo userRolesRepo, IUserManager userManager)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _userRolesRepo = userRolesRepo;
            _userManager = userManager;
        }
        
        
        
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(UserDto userDto)
        {
            
            if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Password))
            {
                return BadRequest("Username and/or password cannot be empty");
            }

            var duplicateUser = await _userRepo.FindUserByUsername(userDto.Username);

            if (duplicateUser != null)
            {
                return BadRequest("Username is already taken");
            }

            try
            {
                var createdUser = await _userRepo.CreateUser(userDto);

                await _userRolesRepo.AssignUserRole(createdUser.Id);

                return Ok(createdUser);
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }


        
        
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(UserDto userDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Password))
                {
                    return BadRequest("Username and/or password cannot be empty");
                }

                var user = await _userRepo.FindUserByUsername(userDto.Username);

                if (user == null)
                {
                    return BadRequest("Username or password is incorrect");
                }

                var verifiedPassword = BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password);

                if (!verifiedPassword)
                {
                    return BadRequest("Username or password is incorrect");
                }

                var rolesList = await _userRolesRepo.GetRoles(user.Id);
                
                var token = _tokenService.GenerateJwtToken(user);

                return Ok(new {token = token.Result, roles = rolesList});
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }

        }




        [HttpPut("updatePassword")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> ChangePasswordByUser(UpdatePasswordDto updatePasswordDto)
        {
            if (updatePasswordDto.CurrentPassword == updatePasswordDto.NewPassword)
            {
                return BadRequest("New password cannot be the same as the current password.");
            }

            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(updatePasswordDto.NewPassword);
            
            var user = await _userRepo.FindUserById(Convert.ToInt32(UserId));

            try
            {
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