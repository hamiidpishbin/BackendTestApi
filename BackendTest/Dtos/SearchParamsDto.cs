namespace BackendTest.Dtos;

public class SearchParamsDto
{
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? MovieName { get; }

    public string? DirectorName { get; }
    public List<string>? Actors { get; set; }
    

    public SearchParamsDto(string movieName, string directorName)
    {
        if (movieName != null) MovieName = movieName.Trim().ToLower();
        if (directorName != null) DirectorName = directorName.Trim().ToLower();
    }
}