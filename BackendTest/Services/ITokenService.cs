using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Services;

public interface ITokenService
{
    Task<string> GenerateJwtToken(User user);
}
