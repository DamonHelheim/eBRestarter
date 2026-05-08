using FluentValidation;

namespace eBRestarter.Core.Application.UseCases.ManageRestarterCycle;

public class ManageRestarterCycleRequestValidator : AbstractValidator<ManageRestarterCycleRequest>
{
    public ManageRestarterCycleRequestValidator()
    {
        RuleFor(x => x.BrowserDisplayName)
            .NotEmpty()
            .WithMessage("Browser display name is required.");

        RuleFor(x => x.RuntimeSeconds)
            .GreaterThan(0)
            .WithMessage("Runtime must be greater than 0 seconds.");

        RuleFor(x => x.PauseSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Pause time cannot be negative.");
    }
}
