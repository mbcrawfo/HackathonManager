using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HackathonManager.Extensions;
using HackathonManager.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace HackathonManager;

public class RequestLoggingMiddleware
{
    private readonly IReadOnlyCollection<MediaTypeHeaderValue> _loggedContentTypes;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly RequestLoggingSettings _settings;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<RequestLoggingSettings> options
    )
    {
        _next = next;
        _logger = logger;
        _settings = options.Value;
        _loggedContentTypes = MediaTypeHeaderValue.ParseList(_settings.ContentTypes).ToReadOnlyCollection();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        KeyValuePair<string, object>[] state =
        [
            new(LogProperties.RequestId, context.TraceIdentifier),
            new(LogProperties.RequestMethod, context.Request.Method),
            new(LogProperties.RequestRoute, context.Request.Path),
            new(LogProperties.UrlPath, context.Request.Path),
            new(LogProperties.UrlQuery, context.Request.QueryString.ToString()),
        ];
        using var scope = _logger.BeginScope(state);

        await LogRequestBody(context.Request, context.RequestAborted);
        await _next(context);
    }

    private async ValueTask LogRequestBody(HttpRequest request, CancellationToken cancellationToken)
    {
        // Serilog level and ILogger levels use the same underlying values.
        var level = (LogLevel)(int)_settings.BodyLogLevel;

        if (!_settings.LogBody || !_logger.IsEnabled(level) || request.ContentLength is null or 0)
        {
            return;
        }

        if (
            !MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType)
            || _loggedContentTypes.All(t => !contentType.IsSubsetOf(t))
        )
        {
            _logger.Log(
                level,
                "Request body {ContentType}, {ContentLength} bytes",
                request.ContentType,
                request.ContentLength
            );
            return;
        }

        if (request.ContentLength > _settings.MaxBodySize)
        {
            _logger.Log(
                level,
                "Request body {ContentType}, {ContentLength} bytes - too large to log",
                request.ContentType,
                request.ContentLength
            );
            return;
        }

        request.EnableBuffering();
        await using var stream = new MemoryStream((int)request.ContentLength);
        await request.Body.CopyToAsync(stream, cancellationToken);
        request.Body.Position = 0;

        var encoding = contentType.Encoding ?? Encoding.UTF8;
        var body = encoding.GetString(stream.GetBuffer());
        _logger.Log(
            level,
            "Request body {ContentType}, {ContentLength} bytes, value: {RequestBody:l}",
            request.ContentType,
            request.ContentLength,
            body
        );
    }
}
