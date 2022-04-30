using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace BackendTest.Dtos;

public class UserDto
{
    [Required(ErrorMessage = "Username and/or password cannot be null or empty!")]
    public string Username { get; }
    
    [Required(ErrorMessage = "Username and/or password cannot be null or empty!")]
    [MinLength(8, ErrorMessage = "Password word must be at least 8 characters!")]
    public string Password { get; }

    public UserDto(string username, string password)
    {
        if (username != null) Username = username.Trim().Replace("'", "").ToLower();
        if (password != null) Password = password.Trim().Replace("'", "");
    }
}