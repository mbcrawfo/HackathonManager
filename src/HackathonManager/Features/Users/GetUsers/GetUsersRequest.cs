using FastEndpoints;
using HackathonManager.Interfaces;

namespace HackathonManager.Features.Users.GetUsers;

public enum UserSearch
{
    Email = 1,
    Name = 2,
}

public enum UserSort
{
    Created = 1,
    CreatedDesc = 2,
    Email = 3,
    EmailDesc = 4,
    Name = 5,
    NameDesc = 6,
}

public sealed record GetUsersRequest(
    [property: QueryParam] UserSearch? Search,
    [property: QueryParam] string? Term,
    [property: QueryParam] UserSort Sort = UserSort.Name,
    [property: QueryParam] int PageSize = Constants.PageSizeDefault,
    [property: QueryParam] string? Cursor = null,
    [property: FromHeader("If-None-Match", isRequired: false)] string? IfNoneMatch = null
) : ICursorRequest;
