using FastIDs.TypeId;
using NodaTime;

namespace HackathonManager.Features.Users;

public sealed record UserDto(TypeId Id, Instant Created, string Email, string DisplayName);
