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
    private readonly IUserRepository _userRepository;
    private const double ExpiryDurationMinutes = 60;

    public  TokenManager(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }
    
    public async Task<string> GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["JWT:Key"];
        var jwtIssuer = _configuration["JWT:Issuer"];
        var jwtAudience = _configuration["JWT:Audience"];

        var userRoles = await _userRepository.GetUserRoles(user.Id);

        var claims = userRoles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
        claims.Add(new Claim("UserId", user.Id.ToString()));


        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);           
        var tokenDescriptor = new JwtSecurityToken(jwtIssuer, jwtAudience, claims, 
            expires: DateTime.Now.AddMinutes(ExpiryDurationMinutes), signingCredentials: credentials);        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}