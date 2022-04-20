using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Controllers;

public class BaseController : ControllerBase
{
    protected string UserId
    {
        get
        {
            return HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "UserId").Value;
        }
    }
}