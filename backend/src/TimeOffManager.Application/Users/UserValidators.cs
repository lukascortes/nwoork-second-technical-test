using FluentValidation;

namespace TimeOffManager.Application.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.AnnualVacationDays).InclusiveBetween(0, 365);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an upper-case letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lower-case letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        When(x => x.Email is not null, () =>
            RuleFor(x => x.Email!).NotEmpty().EmailAddress().MaximumLength(256));

        When(x => x.FullName is not null, () =>
            RuleFor(x => x.FullName!).NotEmpty().MaximumLength(120));

        When(x => x.Role is not null, () =>
            RuleFor(x => x.Role!.Value).IsInEnum());

        When(x => x.AnnualVacationDays is not null, () =>
            RuleFor(x => x.AnnualVacationDays!.Value).InclusiveBetween(0, 365));

        When(x => x.Password is not null, () =>
            RuleFor(x => x.Password!)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(128)
                .Matches("[A-Z]").WithMessage("Password must contain an upper-case letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lower-case letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit."));
    }
}
