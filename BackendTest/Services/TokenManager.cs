using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendTest.Models;
using BackendTest.Repository;
using Microsoft.IdentityModel.Tokens;

namespace BackendTest.Services;

public class TokenManager : ITokenManager
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepo _userRepo;
    private const double ExpiryDurationMinutes = 60;

    public  TokenManager(IConfiguration configuration, IUserRepo userRepo)
    {
        _configuration = configuration;
        _userRepo = userRepo;
    }
    
    public async Task<string> GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["JWT:Key"];
        var jwtIssuer = _configuration["JWT:Issuer"];
        var jwtAudience = _configuration["JWT:Audience"];

        var userRoles = await _userRepo.GetRoles(user.Id);

        var claims = userRoles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
        claims.Add(new Claim("UserId", user.Id.ToString()));


        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);           
        var tokenDescriptor = new JwtSecurityToken(jwtIssuer, jwtAudience, claims, 
            expires: DateTime.Now.AddMinutes(ExpiryDurationMinutes), signingCredentials: credentials);        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);  

    }

    public bool ValidateToken(string key, string issuer, string audience, string token)
    {
        throw new NotImplementedException();
    }
}