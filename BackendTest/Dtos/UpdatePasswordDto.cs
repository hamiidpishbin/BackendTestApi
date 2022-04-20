using System.ComponentModel.DataAnnotations;

namespace BackendTest.Dtos;

public class UpdatePasswordDto
{
    [Required]
    public string CurrentPassword { get; }
    [Required]
    public string NewPassword { get; }

    public UpdatePasswordDto(string currentPassword, string newPassword)
    {
        CurrentPassword = currentPassword.Trim().Replace("'", "");
        NewPassword = newPassword.Trim().Replace("'", "");
    }
}