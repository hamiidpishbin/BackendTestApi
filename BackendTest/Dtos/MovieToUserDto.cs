namespace BackendTest.Dtos;

public class MovieToUserDto
{
    public string Name { get; set; }
    

    public int Year { get; set; }

    public string DirectorName { get; set; }

    public List<string> Actors { get; set; }
}