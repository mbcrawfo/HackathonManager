using FastEndpoints;
using HackathonManager.Extensions;
using HackathonManager.Persistence.Entities;

namespace HackathonManager.Features.Users.CreateUser;

public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
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
