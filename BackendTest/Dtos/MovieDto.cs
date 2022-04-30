using Microsoft.Build.Framework;

namespace BackendTest.Dtos;

public class MovieDto
{
    [Required]
    public string Name { get; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public string DirectorName { get; }

    [Required]
    public List<string> Actors { get; set; }

    public MovieDto(string name, string directorName)
    {
        if (name != null) Name = name.Trim().Replace("'", "").ToLower();
        if (directorName != null) DirectorName = directorName.Trim().Replace("'", "").ToLower();
    }
}