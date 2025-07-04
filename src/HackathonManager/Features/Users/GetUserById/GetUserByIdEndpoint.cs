using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HackathonManager.Features.Users.GetUserById;

public sealed class GetUserByIdEndpoint : Endpoint<GetUserByIdRequest, Results<Ok<UserDto>, NotFound>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/users/{@id}", x => new { x.Id });
        AllowAnonymous();
        Options(b => b.WithVersionSet(ApiTags.Users).HasApiVersion(1));
    }

    /// <inheritdoc />
    public override Task<Results<Ok<UserDto>, NotFound>> ExecuteAsync(GetUserByIdRequest req, CancellationToken ct) =>
        throw new NotImplementedException();
}
