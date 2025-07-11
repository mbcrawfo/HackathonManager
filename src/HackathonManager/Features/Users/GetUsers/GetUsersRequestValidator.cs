using System;
using FastEndpoints;
using FluentValidation;
using HackathonManager.Extensions;
using HackathonManager.Interfaces;
using HackathonManager.Persistence.Entities;

namespace HackathonManager.Features.Users.GetUsers;

public class GetUsersRequestValidator : Validator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleFor(x => x.Search).NotNullWithCode().IsInEnumWithCode().When(x => x.Search is not null);

        RuleFor(x => x.Term)
            .NotNullOrEmptyWithCode()
            .MaximumLengthWithCode(Math.Max(User.EmailMaxLength, User.DisplayNameMaxLength))
            .When(x => x.Search is not null);

        RuleFor(x => x.Sort).NotNullWithCode().IsInEnumWithCode();

        RuleFor(x => x.PageSize).InclusiveBetweenWithCode(Constants.PageSizeMin, Constants.PageSizeMax);

        this.AddCursorRules();
    }
}
