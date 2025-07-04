using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace HackathonManager.Extensions;

public static class EndpointExtensions
{
    [DoesNotReturn]
    public static void ThrowInvalidETagError<TRequest, TResponse>(
        this Endpoint<TRequest, TResponse> endpoint,
        string headerName
    )
        where TRequest : notnull
    {
        endpoint.ThrowError(
            new ValidationFailure
            {
                ErrorCode = ErrorCodes.InvalidETag,
                ErrorMessage = $"Invalid ETag value in {headerName} header.",
            },
            StatusCodes.Status422UnprocessableEntity
        );
    }
}
