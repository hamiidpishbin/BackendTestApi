using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;

namespace BackendTest.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRolesRepo _userRolesRepo;

    private const double ExpiryDurationMinutes = 30;

    public  TokenService(IConfiguration configuration, IUserRolesRepo userRolesRepo)
    {
        _configuration = configuration;
        _userRolesRepo = userRolesRepo;
    }
    public async Task<string> GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["JWT:Key"];
        var jwtIssuer = _configuration["JWT:Issuer"];
        var jwtAudience = _configuration["JWT:Audience"];

        var userRoles = await _userRolesRepo.GetRoles(user.Id);

        var claims = userRoles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();


        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);           
        var tokenDescriptor = new JwtSecurityToken(jwtIssuer, jwtIssuer, claims, 
            expires: DateTime.Now.AddMinutes(ExpiryDurationMinutes), signingCredentials: credentials);        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);  

    }

    public bool ValidateToken(string key, string issuer, string audience, string token)
    {
        throw new NotImplementedException();
    }
}