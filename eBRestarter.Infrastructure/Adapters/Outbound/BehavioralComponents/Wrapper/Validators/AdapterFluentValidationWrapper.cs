using System;
using FluentValidation;

using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Validators;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) wrapping FluentValidation to validate domain models and commands.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Wraps FluentValidation for domain model and command validation in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IInboundPortApplicationValidator{T}"/>.<br/>
/// </para>
/// </summary>
/// <param name="validator">FluentValidation validator instance.</param>
public sealed class AdapterFluentValidationWrapper<T>(
    IValidator<T> validator)
    : IInboundPortApplicationValidator<T>
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected dependencies ──
    private readonly IValidator<T> _validator = validator ?? throw new ArgumentNullException(nameof(validator));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
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
