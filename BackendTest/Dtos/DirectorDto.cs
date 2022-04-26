using Microsoft.Build.Framework;

namespace BackendTest.Dtos;

public class DirectorDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; }

    public DirectorDto()
    {
        
    }

    public DirectorDto(string name)
    {
        Name = name.Trim().Replace("'", "").ToLower();
    }
}