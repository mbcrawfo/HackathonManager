using System.Diagnostics;
using OpenTelemetry;

namespace HackathonManager.Utilities;

/// <summary>
///     Fixes traces showing the route template instead of the actual route.
/// </summary>
/// <remarks>
///     See https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/2580
/// </remarks>
public sealed class HttpRouteTraceProcessor : BaseProcessor<Activity>
{
    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (data.Kind is not ActivityKind.Server)
        {
            return;
        }

        var method = data.GetTagItem("http.request.method")?.ToString();
        var path = data.GetTagItem("url.path")?.ToString();

        if (method is null || path is null)
        {
            return;
        }

        data.DisplayName = $"{method} {path}";
        data.SetTag(LogPropertyNames.RequestRoute, path);
    }
}
