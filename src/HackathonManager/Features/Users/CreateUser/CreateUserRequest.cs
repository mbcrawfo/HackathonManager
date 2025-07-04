using JetBrains.Annotations;

namespace HackathonManager.Features.Users.CreateUser;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CreateUserRequest
{
    /// <summary>
    ///     The user's email address, used as their login identifier.  Must be unique.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    ///     Name that will publicly identify the user.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    ///     The user's password.
    /// </summary>
    public required string Password { get; init; }
}
