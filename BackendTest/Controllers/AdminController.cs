using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BackendTest.Dtos;
using BackendTest.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : BaseController
    {
        private readonly IUserRepo _userRepo;

        public AdminController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        
        
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepo.GetUsers();

            return Ok(users);
        }



        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser(UserDto userDto)
        {
            var duplicateUser = await _userRepo.FindUserByUsername(userDto.Username);

            if (duplicateUser != null)
            {
                return BadRequest("Username is already taken");
            }

            try
            {
                var createdUser = await _userRepo.CreateUser(userDto);

                return Ok(createdUser);
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }



        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepo.FindUserById(id);

                if (user == null)
                {
                    return BadRequest("User not found!");
                }

                await _userRepo.DeleteUser(user);

                return Ok("User was deleted.");
            }
            catch (Exception exception)
            {
                return Problem(exception.Message);
            }
        }



        [HttpPut("EditUser/{id}")]
        public async Task<IActionResult> EditUser(int id, UserDto user)
        {
            try
            {
                var userIdDb = await _userRepo.FindUserById(id);

                if (userIdDb == null)
                {
                    return BadRequest("User not found");
                }

                await _userRepo.AdminEditUser(id, user);

                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
