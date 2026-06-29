using eBRestarter.Core.Application.Ports.Inbound.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using FluentValidation;

namespace eBRestarter.Core.Application.Validators;

public sealed class ConfigureAutoLogonValidator : AbstractValidator<ConfigureAutoLogonRequest>
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
