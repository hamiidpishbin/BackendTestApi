using Microsoft.Build.Framework;

namespace BackendTest.Dtos;

public class Director
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
}