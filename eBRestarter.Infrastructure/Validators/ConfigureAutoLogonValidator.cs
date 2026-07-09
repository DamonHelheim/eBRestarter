using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using FluentValidation;

namespace eBRestarter.Infrastructure.Validators;

public sealed partial class ConfigureAutoLogonValidator : AbstractValidator<ConfigureAutoLogonRequest>
{
    public ConfigureAutoLogonValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .When(x => !x.IsDeactivateAction)
            .WithMessage("Username is required when activating AutoLogon.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .When(x => !x.IsDeactivateAction)
            .WithMessage("Password is required when activating AutoLogon.");
    }
}
