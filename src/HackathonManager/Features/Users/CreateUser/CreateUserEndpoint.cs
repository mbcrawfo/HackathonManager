using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastIDs.TypeId;
using FluentValidation.Results;
using HackathonManager.Extensions;
using HackathonManager.Features.Users.GetUserById;
using HackathonManager.Persistence;
using HackathonManager.Persistence.Entities;
using HackathonManager.Persistence.Enums;
using HackathonManager.Services;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NodaTime;
using Sqids;

namespace HackathonManager.Features.Users.CreateUser;

public sealed class CreateUserEndpoint(
    HackathonDbContext _dbContext,
    SqidsEncoder<uint> _encoder,
    IClock _clock,
    PasswordService _passwordService
) : Endpoint<CreateUserRequest, Results<CreatedAtRoute<UserDto>, ProblemDetails>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
        Description(b =>
        {
            b.WithVersionSet(ApiTags.Users).HasApiVersion(1);
            b.ProducesProblemDetails(StatusCodes.Status409Conflict);
        });

        Summary(s =>
        {
            s.Summary = "Creates a user account.";
            s.Responses[StatusCodes.Status409Conflict] = "The user email or display name is already in use.";
            s.ResponseHeaders.Add(new ResponseHeader(StatusCodes.Status200OK, HeaderNames.ETag));
        });
    }

    /// <inheritdoc />
    public override async Task<Results<CreatedAtRoute<UserDto>, ProblemDetails>> ExecuteAsync(
        CreateUserRequest req,
        CancellationToken ct
    )
    {
        var now = _clock.GetCurrentInstant();
        var user = new User
        {
            Id = TypeIdDecoded.FromUuidV7(ResourceTypes.User, Guid.CreateVersion7(now.ToDateTimeOffset())),
            CreatedAt = now,
            Email = req.Email,
            DisplayName = req.DisplayName,
            PasswordHash = _passwordService.HashPassword(req.Password),
            AuditEvents =
            [
                new UserAudit
                {
                    Timestamp = now,
                    Event = UserAuditEventType.Registration,
                    Metadata = new UserRegistrationMetadataV1(req.Email, req.DisplayName),
                },
            ],
        };

        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            Logger.LogDebug(ex, "Unique constraint error on user creation");

            var propertyName = (ex.GetPostgresException().ConstraintName ?? "") switch
            {
                var x when x.Contains("email") => nameof(CreateUserRequest.Email).Camelize(),
                var x when x.Contains("display_name") => nameof(CreateUserRequest.DisplayName).Camelize(),
                _ => throw new InvalidOperationException("Unexpected unique constraint error", ex),
            };

            return new ProblemDetails(
                [
                    new ValidationFailure
                    {
                        ErrorCode = ErrorCodes.UniqueValueRequired,
                        ErrorMessage = $"Value provided for '{propertyName}' is already in use.",
                        PropertyName = propertyName,
                    },
                ],
                StatusCodes.Status409Conflict
            );
        }

        HttpContext.Response.Headers.ETag = new StringValues(_encoder.Encode(user.RowVersion));

        var result = new UserDto(user.Id.Encode(), user.CreatedAt, user.Email, user.DisplayName);
        return TypedResults.CreatedAtRoute(result, nameof(GetUserByIdEndpoint), new { Id = result.Id.ToString() });
    }
}
