using BackendTest.Dtos;

namespace BackendTest.Services;

public class SearchParamsValidator : ISearchParamsValidator
{
    public bool IsYearRangeValid(SearchParamsDto searchParams)
    {
        return searchParams.StartYear != null && searchParams.EndYear != null &&
               searchParams.StartYear >= searchParams.EndYear && searchParams.StartYear > 0 && searchParams.EndYear > 0;
    }

    public bool IsMovieNameValid(SearchParamsDto searchParams)
    {
        searchParams.MovieName = searchParams.MovieName?.Trim().Replace("'", "").ToLower();
        return !string.IsNullOrWhiteSpace(searchParams.MovieName);
    }

    public bool IsDirectorNameValid(SearchParamsDto searchParams)
    {
        searchParams.DirectorName = searchParams.DirectorName?.Trim().Replace("'", "").ToLower();
        return !string.IsNullOrWhiteSpace(searchParams.DirectorName);
    }

    public bool IsActorsListValid(SearchParamsDto searchParams)
    {
        if (searchParams.Actors == null) return false;
        
        searchParams.Actors = searchParams.Actors.Select(actor => actor.Trim().Replace("'", "").ToLower()).Where(actor => actor != "").ToList();

        return searchParams.Actors.Any();
    }
}