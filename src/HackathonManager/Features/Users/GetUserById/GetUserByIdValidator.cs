using FastEndpoints;
using FluentValidation;
using HackathonManager.Extensions;

namespace HackathonManager.Features.Users.GetUserById;

public sealed class GetUserByIdValidator : Validator<GetUserByIdRequest>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.Id).MustBeIdOfType(ResourceTypes.User);

        RuleFor(x => x.IfNoneMatch)
            .NotEmptyWithCode()
            .MaximumLengthWithCode(Constants.ETagMaxLength)
            .When(x => x.IfNoneMatch is not null);
    }
}
