using FluentValidation;

namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public class ConfigureAutoLogonRequestValidator : AbstractValidator<ConfigureAutoLogonRequest>
{
    public ConfigureAutoLogonRequestValidator()
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
