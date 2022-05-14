using BackendTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers;

public class BaseController : ControllerBase
{
    protected int UserId
    {
        get
        {
            var user = (UserWithRoles)HttpContext.Items["User"]!;
            return Convert.ToInt32(user.Id);
        }
    }
}