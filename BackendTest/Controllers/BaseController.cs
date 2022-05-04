using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers;

public class BaseController : ControllerBase
{
    
    protected int UserId
    {
        get
        {
            return Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value);
        }
    }
}