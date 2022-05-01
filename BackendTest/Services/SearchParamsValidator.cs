using BackendTest.Dtos;

namespace BackendTest.Services;

public class SearchParamsValidator : ISearchParamsValidator
{
    public bool IsYearRangeValid(SearchParamsDto searchParams)
    {
        return searchParams.StartYear != null && searchParams.EndYear != null && searchParams.StartYear >= searchParams.EndYear;
    }

    public bool IsMovieNameValid(SearchParamsDto searchParams)
    {
        return !string.IsNullOrWhiteSpace(searchParams.MovieName);
    }

    public bool IsDirectorNameValid(SearchParamsDto searchParams)
    {
        return !string.IsNullOrWhiteSpace(searchParams.DirectorName);
    }

    public bool IsActorsListValid(SearchParamsDto searchParams)
    {
        return searchParams.Actors != null && searchParams.Actors.Any();
    }
}