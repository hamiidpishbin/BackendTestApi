using System.ComponentModel.DataAnnotations;

namespace BackendTest.Dtos;

public class UserDto
{
    [Required]
    public string Username { get; }
    [Required]
    public string Password { get; }

    public UserDto(string username, string password)
    {
        Username = username.Trim().Replace("'", "");
        Password = password.Trim().Replace("'", "");
    }
}