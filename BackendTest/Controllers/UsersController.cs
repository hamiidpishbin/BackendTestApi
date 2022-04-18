using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendTest.Dtos;
using BackendTest.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepo _userRepo;

    public UsersController(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userRepo.GetUsers();

            return Ok(users);
        }
        catch (Exception exception)
        {
            return Problem(exception.Message);
        }    
    }

    
}