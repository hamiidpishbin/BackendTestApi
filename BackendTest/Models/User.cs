using System.ComponentModel.DataAnnotations;

namespace BackendTest.Models;

public class User
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}