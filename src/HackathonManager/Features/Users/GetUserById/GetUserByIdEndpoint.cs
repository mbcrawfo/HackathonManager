using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using HackathonManager.Extensions;
using HackathonManager.Persistence;
using HackathonManager.Utilities.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Sqids;

namespace HackathonManager.Features.Users.GetUserById;

public sealed class GetUserByIdEndpoint : Endpoint<GetUserByIdRequest, Results<Ok<UserDto>, NotFound, NotModified>>
{
    private readonly HackathonDbContext _dbContext;
    private readonly SqidsEncoder<uint> _encoder;

    /// <inheritdoc />
    public GetUserByIdEndpoint(HackathonDbContext dbContext, SqidsEncoder<uint> encoder)
    {
        _dbContext = dbContext;
        _encoder = encoder;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Get("/users/{@id}", x => new { x.Id });
        // TODO: Should require auth
        AllowAnonymous();

        Description(b =>
        {
            b.WithVersionSet(ApiTags.Users).HasApiVersion(1);
            b.Produces(StatusCodes.Status304NotModified);
        });

        Summary(s =>
        {
            s.Summary = "Gets a user by their id.";
            s.ResponseHeaders.Add(new ResponseHeader(StatusCodes.Status200OK, HeaderNames.ETag));
        });
    }

    /// <inheritdoc />
    public override async Task<Results<Ok<UserDto>, NotFound, NotModified>> ExecuteAsync(
        GetUserByIdRequest req,
        CancellationToken ct
    )
    {
        uint? rowVersion = null;
        if (req.IfNoneMatch is not null && !_encoder.DecodeAndValidate(req.IfNoneMatch, out rowVersion))
        {
            this.ThrowInvalidETagError(HeaderNames.IfNoneMatch);
        }

        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == req.Id.Decode(), ct);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        if (rowVersion is not null && rowVersion == user.RowVersion)
        {
            return NotModified.Instance;
        }

        var userDto = new UserDto(
            user.Id.Encode(),
            user.CreatedAt,
            user.Email,
            user.DisplayName,
            _encoder.Encode(user.RowVersion)
        );

        HttpContext.Response.Headers.ETag = new StringValues(userDto.Version);
        return TypedResults.Ok(userDto);
    }
}
