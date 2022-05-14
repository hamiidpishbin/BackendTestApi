using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Services;

public interface ITokenManager
{
    Task<string> GenerateJwtToken(User user);
    UserWithRoles ValidateJwtToken(string? token);
}
