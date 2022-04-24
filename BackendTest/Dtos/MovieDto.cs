using Microsoft.Build.Framework;

namespace BackendTest.Dtos;

public class MovieDto
{
    [Required]
    public string Name { get; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public string DirectorName { get; set; }

    [Required] public List<string> Actors { get; set; }

    public MovieDto(string name)
    {
        Name = name.Trim().Replace("'", "").ToLower();
    }
}