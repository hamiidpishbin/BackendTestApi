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
        var userRoles = await _userRepository.GetUserRoles(user.Id);

        var claims = userRoles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
        claims.Add(new Claim("UserId", user.Id.ToString()));
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(ExpiryDurationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }

    public UserWithRoles ValidateJwtToken(string? token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false, 
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            var jwtToken = (JwtSecurityToken) validatedToken;
            
            var userId = int.Parse(jwtToken.Claims.Single(x => x.Type == "UserId").Value);
            var userRoles = jwtToken.Claims.Where(x => x.Type == "role").Select(y => y.Value).ToList();
            
            var user = new UserWithRoles
            {
                Id = userId,
                Roles = userRoles
            };

            return user;
        }
        catch (Exception exception)
        {
            throw;
        }
    }
}