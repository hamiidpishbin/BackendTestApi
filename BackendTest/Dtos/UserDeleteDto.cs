using System.ComponentModel.DataAnnotations;

namespace BackendTest.Dtos;

public class UserDeleteDto
{
    [Required] public string Username { get; }

    public UserDeleteDto(string username)
    {
        Username = username.Trim().Replace("'", "").ToLower();
    }
}