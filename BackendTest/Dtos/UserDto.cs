namespace BackendTest.Dtos;

public class UserDto
{
    public string Username { get; }
    public string Password { get; }

    public UserDto(string username, string password)
    {
        Username = username.Trim().Replace("'", "");
        Password = password.Trim().Replace("'", "");
    }
}