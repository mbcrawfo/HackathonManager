using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace HackathonManager.Utilities.Results;

public sealed class NotModified : IResult, IStatusCodeHttpResult, IEndpointMetadataProvider
{
    public static readonly NotModified Instance = new();

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode!.Value;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public int? StatusCode => StatusCodes.Status304NotModified;

    /// <inheritdoc />
    public static void PopulateMetadata(MethodInfo _, EndpointBuilder builder)
    {
        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status304NotModified));
    }
}
