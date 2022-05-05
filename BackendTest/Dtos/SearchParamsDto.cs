namespace BackendTest.Dtos;

public class SearchParamsDto
{
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? MovieName { get; set; }

    public string? DirectorName { get; set; }
    public List<string>? Actors { get; set; }
}