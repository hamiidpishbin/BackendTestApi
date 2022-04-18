using BackendTest.Dtos;

namespace BackendTest.Services;

public interface ITokenService
{
    string BuildToken(string key, string issuer, UserDto user);
    bool ValidateToken(string key, string issuer, string audience, string token);
}
