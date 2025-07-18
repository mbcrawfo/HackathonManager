using JetBrains.Annotations;

namespace HackathonManager.Features.Users.CreateUser;

/// <summary>
///     Request to create a new user account.
/// </summary>
/// <param name="Email">
///     The user's email address, used as their login identifier.  Must be unique.
/// </param>
/// <param name="DisplayName">
///     Name that will publicly identify the user.
/// </param>
/// <param name="Password">
///     The user's password.
/// </param>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed record CreateUserRequest(string Email, string DisplayName, string Password);
