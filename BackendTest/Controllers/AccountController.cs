using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendTest.Data;
using BackendTest.Dtos;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepo _userRepo;


        public AccountController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser(UserDto userDto)
        {
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
    }
}
