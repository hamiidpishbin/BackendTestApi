namespace BackendTest.Dtos;

public class SearchParamsDto
{
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? MovieName { get; set; }

    public string? DirectorName { get; set; }
    public List<string>? Actors { get; set; }
    

    // public SearchParamsDto(string movieName, string directorName, List<string> actors)
    // {
    //     if (movieName != null) MovieName = movieName.Trim().Replace("'","").ToLower();
    //     
    //     if (directorName != null) DirectorName = directorName.Trim().Replace("'","").ToLower();
    //
    //     if (actors != null && actors.Any())
    //     {
    //         Actors = actors.Select(actor => actor.Trim().Replace("'", "").ToLower()).ToList();
    //     }
    // }
}