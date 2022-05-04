using System.ComponentModel.DataAnnotations;

namespace BackendTest.Dtos;

public class UpdatePasswordDto
{
    [Required(ErrorMessage = "Current password cannot be null or empty")]
    public string CurrentPassword { get; }
    
    [Required(ErrorMessage = "New password cannot be null or empty")]
    public string NewPassword { get; }

    public UpdatePasswordDto(string currentPassword, string newPassword)
    {
        if (currentPassword != null) CurrentPassword = currentPassword.Trim().Replace("'", "");
        if (newPassword != null) NewPassword = newPassword.Trim().Replace("'", "");
    }
}