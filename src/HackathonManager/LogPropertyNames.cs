namespace HackathonManager;

/// <summary>
///     Constants defining shared properties for structured logging and traces.
/// </summary>
public static class LogPropertyNames
{
    public const string RequestId = "http.request.id";
    public const string RequestMethod = "http.request.method";
    public const string RequestRoute = "http.route";
    public const string UrlPath = "url.path";
    public const string UrlQuery = "url.query";
}
