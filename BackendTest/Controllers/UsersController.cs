using BackendTest.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "USER")]
    [Authorize(Roles = "ADMIN")]
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