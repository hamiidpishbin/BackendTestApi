using BackendTest.Models;
using BackendTest.Services;

namespace BackendTest.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenManager _tokenManager;

    public JwtMiddleware(RequestDelegate next, ITokenManager tokenManager)
    {
        _next = next;
        _tokenManager = tokenManager;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path == "/api/account/login" || context.Request.Path == "/api/account/signup")
        {
            await _next(context);
        }
        else
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            try
            {
                var user = _tokenManager.ValidateJwtToken(token);

                context.Items.Add("User", user);
                
                await _next(context);
            }
            catch (Exception exception)
            {
                Console.WriteLine("MIDDLEWARE EXCEPTION SOURCE: " + exception.Source);

                if (exception is ArgumentException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new ClientMessage {ErrorMessage = "Invalid token"});    
                }
            }
        }
    }
}

public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}