namespace HackathonManager.Interfaces;

public interface ICursorResponse
{
    /// <summary>
    ///     Search cursor to retrieve the previous page of results.
    /// </summary>
    string? PrevCursor { get; }

    /// <summary>
    ///     Search cursor to retrieve the next page of results.
    /// </summary>
    string? NextCursor { get; }
}
