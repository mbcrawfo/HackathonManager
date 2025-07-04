using FastIDs.TypeId;
using Microsoft.AspNetCore.Mvc;

namespace HackathonManager.Features.Users.GetUserById;

public sealed class GetUserByIdRequest
{
    [FromRoute]
    public TypeId Id { get; init; }
}
