using System.Text.Json.Serialization;

namespace HackathonManager.Features.Users.GetUsers;

public sealed record GetUsersCursor(
    [property: JsonPropertyName("a")] UserSearch? Search,
    [property: JsonPropertyName("b")] string? SearchTerm,
    [property: JsonPropertyName("c")] UserSort Sort,
    [property: JsonPropertyName("d")] int PageSize,
    [property: JsonPropertyName("e")] string SortValue
);
