using BackendTest.Dtos;

namespace BackendTest.Services;

public interface ISearchParamsValidator
{
    bool IsYearRangeValid(SearchParamsDto searchParams);
    bool IsMovieNameValid(SearchParamsDto searchParams);
    bool IsDirectorNameValid(SearchParamsDto searchParams);
    bool IsActorsListValid(SearchParamsDto searchParams);
}