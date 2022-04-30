namespace BackendTest.Dtos;

public class SingleRowMovie
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public string Name { get; set; }
    public int Year { get; set; }
    public string DirectorName { get; set; }
    public string ActorName { get; set; }
}