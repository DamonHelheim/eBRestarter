using eBRestarter.Core.Application.Models.Records;
using FluentValidation;

namespace eBRestarter.Core.Application.Validators;

public sealed class ManageRestarterCycleValidator : AbstractValidator<ManageRestarterCycleRequest>
{
    public ManageRestarterCycleValidator()
    {
        RuleFor(x => x.BrowserType).IsInEnum().WithMessage("A valid browser type must be selected.");

        RuleFor(x => x.RuntimeSeconds)
            .GreaterThan(0)
            .WithMessage("Runtime must be greater than 0 seconds.");

        RuleFor(x => x.PauseSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Pause time cannot be negative.");
    }
}


