using System.ComponentModel.DataAnnotations;

namespace BackendTest.Dtos;

public class MovieDto
{
    [Required]
    public string Name { get; }
    
    [Required]
    public int Year { get; set; }

    // private int _ID;

    // public int ID
    // {
    //     get
    //     {
    //         return _ID;
    //     }
    //     set
    //     {
    //         _ID = value;
    //     }
    // }

    [Required]
    public string DirectorName { get; }

    [Required]
    public List<string> Actors { get; }

    public MovieDto(string name, string directorName, List<string> actors)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name.Trim().Replace("'", "").ToLower();
        
        if (!string.IsNullOrWhiteSpace(directorName)) DirectorName = directorName.Trim().Replace("'", "").ToLower();

        if (actors == null || !actors.Any()) return;
        Actors = actors.Select(actor => actor.Trim().Replace("'", "").ToLower()).Where(actor => actor != "").ToList();
    }
}