using BackendTest.Dtos;

namespace BackendTest.Helpers;

public interface IMovieHelper
{
    public List<MovieInDbDto> MergeActorNames(IEnumerable<RawMovie> movies);
}