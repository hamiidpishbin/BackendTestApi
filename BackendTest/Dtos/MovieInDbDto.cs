using Microsoft.Build.Framework;

namespace BackendTest.Dtos;

public class MovieInDbDto
{
    public int UserId { get; set; }
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public string DirectorName { get; set; }

    [Required] public List<string> Actors { get; set; }
}