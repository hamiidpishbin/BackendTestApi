namespace BackendTest.Dtos;

public class UserMoviesForAdminDto
{
    public int UserId { get; set; }
    
    public int MovieId { get; set; }
    
    public string Name { get; set; }
    
    public int Year { get; set; }
    
    public string DirectorName { get; set; }

    public List<string> Actors { get; set; }
}