using FastEndpoints;
using FastIDs.TypeId;
using JetBrains.Annotations;

namespace HackathonManager.Features.Users.GetUserById;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class GetUserByIdRequest
{
    [RouteParam]
    public TypeId Id { get; init; }

    [FromHeader("If-None-Match", isRequired: false)]
    public string? IfNoneMatch { get; init; }
}
