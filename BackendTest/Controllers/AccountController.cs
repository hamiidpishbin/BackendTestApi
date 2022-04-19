using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendTest.Data;
using BackendTest.Dtos;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using BackendTest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IUserRolesRepo _userRolesRepo;

        public AccountController(IUserRepo userRepo, ITokenService tokenService, IUserRolesRepo userRolesRepo)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _userRolesRepo = userRolesRepo;
        }
        
        
        
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Username and/or password cannot be empty");
            }

            var duplicateUser = _userRepo.CheckExistingUser(userDto.Username);

            if (duplicateUser.Result != null)
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
                if (!ModelState.IsValid)
                {
                    return BadRequest("Username and/or password cannot be empty");
                }

                var user = await _userRepo.CheckExistingUser(userDto.Username);

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

                return Ok(new {token = token, roles = rolesList});
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }

        }
    }
}
