using FluentValidation;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.TimeOffRequests;

public sealed class CreateTimeOffRequestValidator : AbstractValidator<CreateTimeOffRequestRequest>
{
    public CreateTimeOffRequestValidator(IDateTimeProvider clock)
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Reason).MaximumLength(500);

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(_ => clock.Today)
            .WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date cannot be before start date.");
    }
}

public sealed class UpdateRequestStatusValidator : AbstractValidator<UpdateRequestStatusRequest>
{
    public UpdateRequestStatusValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is RequestStatus.Approved or RequestStatus.Rejected)
            .WithMessage("Status can only be set to Approved or Rejected.");
    }
}
