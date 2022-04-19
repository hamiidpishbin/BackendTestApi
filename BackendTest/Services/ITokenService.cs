using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    bool ValidateToken(string key, string issuer, string audience, string token);
}
