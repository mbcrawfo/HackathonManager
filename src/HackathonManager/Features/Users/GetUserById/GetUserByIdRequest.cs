using FastEndpoints;
using FastIDs.TypeId;
using JetBrains.Annotations;

namespace HackathonManager.Features.Users.GetUserById;

/// <summary>
///     Request to get a user by their ID.
/// </summary>
/// <param name="Id">
///     The ID of the user to retrieve.
/// </param>
/// <param name="IfNoneMatch">
///     Optional ETag value for conditional requests.
/// </param>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed record GetUserByIdRequest(
    [property: RouteParam] TypeId Id,
    [property: FromHeader("If-None-Match", isRequired: false)] string? IfNoneMatch
);
