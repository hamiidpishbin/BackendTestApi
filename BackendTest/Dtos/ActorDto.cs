using Microsoft.Build.Framework;

namespace BackendTest.Dtos;

public class ActorDto
{
    public int Id { get; init; }
    [Required] public string Name { get; }

    public ActorDto(string name)
    {
        Name = name.Trim().Replace("'", "").ToLower();
    }
}