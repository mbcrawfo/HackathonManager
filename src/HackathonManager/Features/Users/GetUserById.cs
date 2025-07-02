using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastIDs.TypeId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HackathonManager.Features.Users;

public sealed class GetUserByIdRequest
{
    [FromRoute]
    public TypeId Id { get; init; }
}

public sealed class GetUserById : Endpoint<GetUserByIdRequest, Results<Ok<UserDto>, NotFound>>
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
