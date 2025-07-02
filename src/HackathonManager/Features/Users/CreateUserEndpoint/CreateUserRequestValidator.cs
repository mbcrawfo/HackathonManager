using FastEndpoints;
using HackathonManager.Extensions;
using HackathonManager.Persistence.Entities;

namespace HackathonManager.Features.Users.CreateUserEndpoint;

public sealed class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotNullWithCode()
            .EmailAddressWithCode()
            .LengthWithCode(User.EmailMinLength, User.EmailMaxLength);

        RuleFor(x => x.DisplayName)
            .NotNullWithCode()
            .LengthWithCode(User.DisplayNameMinLength, User.DisplayNameMaxLength);
    }
}
