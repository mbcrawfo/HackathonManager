using System.Diagnostics.CodeAnalysis;
using FastIDs.TypeId;
using NodaTime;

namespace HackathonManager.Features.Users;

/// <summary>
///     Basic user account information.
/// </summary>
/// <param name="Id">
///     The id of this resource.
/// </param>
/// <param name="Created">
///     The timestamp when the user registered their account.
/// </param>
/// <param name="Email">
///     The user's email address.
/// </param>
/// <param name="DisplayName">
///     The user's name shown in the UI.
/// </param>
/// <param name="Version">
///     The version of this resource returned in the ETag header.
/// </param>
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record UserDto(TypeId Id, Instant Created, string Email, string DisplayName, string Version);
