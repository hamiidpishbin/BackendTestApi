namespace BackendTest.Dtos;

public class ChangePasswordDto
{
    public string Password { get; }

    public ChangePasswordDto(string password)
    {
        Password = password.Trim().Replace("'", "");
    }
}