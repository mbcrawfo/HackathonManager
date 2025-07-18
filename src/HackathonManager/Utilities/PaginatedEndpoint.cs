using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FastEndpoints;
using HackathonManager.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Utilities;

public abstract class PaginatedEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : ICursorRequest
{
    protected bool TryDecodeCursor<TCursor>(string? value, [NotNullWhen(true)] out TCursor? cursor)
        where TCursor : class
    {
        cursor = null;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        try
        {
            cursor = PaginationCursor.Decode<TCursor>(value);
            return true;
        }
        catch (JsonException ex)
        {
            Logger.LogInformation(ex, "Error decoding cursor {RawCursor}", value);
            ThrowInvalidCursor();

            // Static analysis doesn't seem to recognize that Throw... doesn't return.
            return false;
        }
        catch (FormatException ex)
        {
            Logger.LogInformation(ex, "Error decoding cursor {RawCursor}", value);
            ThrowInvalidCursor();

            // Static analysis doesn't seem to recognize that Throw... doesn't return.
            return false;
        }
    }

    [DoesNotReturn]
    protected void ThrowInvalidCursor() =>
        ThrowError(
            "Invalid pagination cursor",
            ErrorCodes.InvalidPaginationCursor,
            statusCode: StatusCodes.Status400BadRequest
        );
}
