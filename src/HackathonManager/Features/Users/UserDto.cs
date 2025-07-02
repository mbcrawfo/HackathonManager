using FastIDs.TypeId;
using NodaTime;

namespace HackathonManager.Features.Users;

public sealed record UserDto(TypeId Id, Instant Created, Instant Updated, string Email, string DisplayName);
