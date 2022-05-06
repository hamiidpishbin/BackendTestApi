using BackendTest.Dtos;

namespace BackendTest.Helpers;

public class MovieHelper : IMovieHelper
{
    public List<MovieInDbDto> MergeActorNames(IEnumerable<SingleRowMovie> movies)
    {
        var movieDictionary = new Dictionary<int, MovieInDbDto>();
       
        foreach (var movie in movies)
        {
            if (!movieDictionary.ContainsKey(movie.MovieId))
            {
                movieDictionary.Add(movie.MovieId, new MovieInDbDto
                {
                    Id = movie.MovieId,
                    Name = movie.Name,
                    Year = movie.Year,
                    DirectorName = movie.DirectorName,
                    Actors = new List<string>(){movie.ActorName}
                });
            }
            else
            {
                movieDictionary[movie.MovieId].Actors.Add(movie.ActorName);
            }
        }
       
        return movieDictionary.Values.ToList();
    }
}