using System;

using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for configuring Windows AutoLogon and passwordless authentication modes.
/// </summary>
public sealed class ConfigureAutoLogonUseCase(
    IOutboundPortOsAutoLogonRepository autoLogonPort,
    IOutboundPortCredentialValidationProvider credentialValidationPort,
    IOutboundPortSystemInfoProvider systemInfoPort,
    IInboundPortApplicationValidator<ConfigureAutoLogonRequest> validator) : IUseCaseConfigureAutoLogon
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string StatusKey = "Status";

    private const string ErrorMessageAdminRequiredActivation = "Administrator rights required to disable passwordless mode.";
    private const string ErrorMessageAdminRequiredDeactivation = "Administrator rights required to restore passwordless mode.";
    private const string ErrorMessageCredentialsMissing = "Credentials missing.";
    private const string ErrorMessageDomainError = "Domain error.";
    private const string ErrorMessageInvalidCredentials = "Invalid credentials.";
    private const string ErrorMessageValidationFailed = "Validation failed.";
    private const string ErrorMessageWindowsHelloBlockActive = "Windows Hello Passwordless Mode ist aktiv. AutoLogon nicht möglich.";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortOsAutoLogonRepository _autoLogonPort = autoLogonPort ?? throw new ArgumentNullException(nameof(autoLogonPort));
    private readonly IOutboundPortCredentialValidationProvider _credentialValidationPort = credentialValidationPort ?? throw new ArgumentNullException(nameof(credentialValidationPort));
    private readonly IOutboundPortSystemInfoProvider _systemInfoPort = systemInfoPort ?? throw new ArgumentNullException(nameof(systemInfoPort));
    private readonly IInboundPortApplicationValidator<ConfigureAutoLogonRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Executes the AutoLogon configuration process based on the provided request parameters.
    /// </summary>
    /// <param name="request">The request detailing activation or deactivation requirements and credentials.</param>
    /// <returns>A <see cref="Result{T}"/> containing the resulting <see cref="AutoLogonResultStatus"/>.</returns>
    public Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result.Fail(new Error(ErrorMessageValidationFailed)
                .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
        }

        try
        {
            return request.IsDeactivateAction
                ? HandleDeactivation(request)
                : HandleActivation(request);
        }
        catch (InvalidOperationException)
        {
            return Result.Fail(new Error(ErrorMessageDomainError)
                .WithMetadata(StatusKey, AutoLogonResultStatus.DomainError));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex)
                .WithMetadata(StatusKey, AutoLogonResultStatus.UnexpectedError));
        }
    }

    private Result<AutoLogonResultStatus> HandleDeactivation(ConfigureAutoLogonRequest request)
    {
        if (request.RestorePasswordlessMode)
        {
            if (!_systemInfoPort.IsUserAdministrator())
            {
                return Result.Fail(new Error(ErrorMessageAdminRequiredDeactivation)
                    .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));
            }

            _autoLogonPort.SetPasswordlessAuth(true);
        }

        _autoLogonPort.DisableAutoLogon();
        return Result.Ok(AutoLogonResultStatus.Deactivated);
    }

    private Result<AutoLogonResultStatus> HandleActivation(ConfigureAutoLogonRequest request)
    {
        if (request.DisablePasswordlessMode)
        {
            if (!_systemInfoPort.IsUserAdministrator())
            {
                return Result.Fail(new Error(ErrorMessageAdminRequiredActivation)
                    .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));
            }

            _autoLogonPort.SetPasswordlessAuth(false);
        }

        if (_autoLogonPort.IsPasswordlessAuthEnabled())
        {
            return Result.Fail(new Error(ErrorMessageWindowsHelloBlockActive)
                .WithMetadata(StatusKey, AutoLogonResultStatus.WindowsHelloBlockActive));
        }

        if (string.IsNullOrWhiteSpace(request.Username) || request.Password is null)
        {
            return Result.Fail(new Error(ErrorMessageCredentialsMissing)
                .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
        }

        string domain = request.Domain ?? string.Empty;
        var isValid = _credentialValidationPort.ValidateCredentials(request.Username, domain, request.Password);

        if (!isValid)
        {
            return Result.Fail(new Error(ErrorMessageInvalidCredentials)
                .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
        }

        _autoLogonPort.EnableAutoLogon(request.Username, domain, request.Password);
        return Result.Ok(AutoLogonResultStatus.Activated);
    }
}
