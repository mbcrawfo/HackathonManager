using System.Collections.Generic;
using HackathonManager.Interfaces;

namespace HackathonManager.Features.Users.GetUsers;

public sealed record GetUsersResponseDto(IEnumerable<UserDto> Users, string? PrevCursor, string? NextCursor)
    : ICursorResponse;
