using System.ComponentModel.DataAnnotations;

namespace BackendTest.Models;

public class User
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}