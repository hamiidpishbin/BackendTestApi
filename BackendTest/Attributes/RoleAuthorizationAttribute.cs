using BackendTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackendTest.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class RoleAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public RoleAuthorizationAttribute(string role)
    {
        _role = role;
    }


    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        var user = (UserWithRoles)context.HttpContext.Items["User"]!;

        if (user.Roles.Contains(_role))
            return;
        
        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.HttpContext.Response.WriteAsJsonAsync(new ClientMessage {ErrorMessage = "Unauthorized"});
    }
}