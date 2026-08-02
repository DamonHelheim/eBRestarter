using System;
using FluentValidation;

using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Validators;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) wrapping FluentValidation to validate domain models and commands.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Validierung via FluentValidation-Bibliothek.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IInboundPortApplicationValidator{T}"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Port implementiert, um externes Validierungs-Framework-Verhalten für den Core bereitzustellen.
/// </para>
/// </summary>
public sealed class AdapterFluentValidationWrapper<T>(
    IValidator<T> validator)
    : IInboundPortApplicationValidator<T>
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly IValidator<T> _validator = validator ?? throw new ArgumentNullException(nameof(validator));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public ValidationResult Validate(T request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var fluentValidationResult = _validator.Validate(request);

        if (fluentValidationResult.IsValid)
        {
            return ValidationResult.Success();
        }

        var validationErrors = fluentValidationResult.Errors.ConvertAll(validationFailure => new ValidationError(validationFailure.ErrorMessage));

        return new ValidationResult(false, validationErrors);
    }
}
