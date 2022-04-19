using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;

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

        string firstRole;

        Claim[] claims;

        if (userRoles.Count == 1)
        {
            firstRole = userRoles[0];
            claims = new[] {
                new Claim(ClaimTypes.Role, firstRole),
            };   
        }
        else
        {
            firstRole = userRoles[0];
            var secondRole = userRoles[1];
            claims = new[] {
                new Claim(ClaimTypes.Role, firstRole),
                new Claim(ClaimTypes.Role, secondRole)
            };
        }

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