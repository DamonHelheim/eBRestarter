using System;

using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for configuring Windows AutoLogon and passwordless authentication modes.
/// </summary>
/// <param name="autoLogonPort">Outbound port for registry auto-logon operations.</param>
/// <param name="credentialValidationPort">Outbound port for validating user credentials.</param>
/// <param name="logger">Application logger instance.</param>
/// <param name="systemInfoPort">Outbound port for system information and administrator privilege checks.</param>
/// <param name="validator">Validator for AutoLogon requests.</param>
public sealed class ConfigureAutoLogonUseCase(
    IOutboundPortOsAutoLogonRepository autoLogonPort,
    IOutboundPortCredentialValidationProvider credentialValidationPort,
    IOutboundPortApplicationLogger<ConfigureAutoLogonUseCase> logger,
    IOutboundPortSystemInfoProvider systemInfoPort,
    IInboundPortApplicationValidator<ConfigureAutoLogonRequest> validator) : IUseCaseConfigureAutoLogon
{
    private const string ErrorMessageAdminRequiredActivation = "Administrator rights required to disable passwordless mode.";
    private const string ErrorMessageAdminRequiredDeactivation = "Administrator rights required to restore passwordless mode.";
    private const string ErrorMessageCredentialsMissing = "Credentials missing.";
    private const string ErrorMessageDomainError = "Domain error.";
    private const string ErrorMessageInvalidCredentials = "Invalid credentials.";
    private const string ErrorMessageValidationFailed = "Validation failed.";
    private const string ErrorMessageWindowsHelloBlockActive = "Windows Hello Passwordless Mode ist aktiv. AutoLogon nicht möglich.";
    private const string StatusKey = "Status";

    private readonly IOutboundPortOsAutoLogonRepository _autoLogonPort = autoLogonPort ?? throw new ArgumentNullException(nameof(autoLogonPort));
    private readonly IOutboundPortCredentialValidationProvider _credentialValidationPort = credentialValidationPort ?? throw new ArgumentNullException(nameof(credentialValidationPort));
    private readonly IOutboundPortApplicationLogger<ConfigureAutoLogonUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOutboundPortSystemInfoProvider _systemInfoPort = systemInfoPort ?? throw new ArgumentNullException(nameof(systemInfoPort));
    private readonly IInboundPortApplicationValidator<ConfigureAutoLogonRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <inheritdoc />
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
        catch (InvalidOperationException exception)
        {
            _logger.LogError(
                LogEventIds.Security.AutoLogonConfigurationFailed,
                exception,
                "AutoLogon configuration failed while reaching the domain controller.");

            return Result.Fail(new Error(ErrorMessageDomainError)
                .WithMetadata(StatusKey, AutoLogonResultStatus.DomainError));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                LogEventIds.Security.AutoLogonConfigurationFailed,
                ex,
                "AutoLogon configuration failed with an unexpected error.");

            return Result.Fail(new ExceptionalError(ex)
                .WithMetadata(StatusKey, AutoLogonResultStatus.UnexpectedError));
        }
    }

    /// <summary>
    /// Handles deactivation of AutoLogon and optional restoration of passwordless authentication mode.
    /// </summary>
    private Result<AutoLogonResultStatus> HandleDeactivation(ConfigureAutoLogonRequest request)
    {
        if (request.RestorePasswordlessMode)
        {
            if (!_systemInfoPort.IsUserAdministrator())
                return Result.Fail(new Error(ErrorMessageAdminRequiredDeactivation)
                    .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));

            _autoLogonPort.SetPasswordlessAuth(true);
        }

        _autoLogonPort.DisableAutoLogon();
        return Result.Ok(AutoLogonResultStatus.Deactivated);
    }

    /// <summary>
    /// Validates all preconditions and credentials before triggering AutoLogon activation.
    /// </summary>
    /// <remarks>
    /// Validates all preconditions (administrator privileges, completeness, credential validity) before executing system configuration changes.
    /// Explicitly restores passwordless mode if activation fails during execution.
    /// </remarks>
    private Result<AutoLogonResultStatus> HandleActivation(ConfigureAutoLogonRequest request)
    {
        if (request.DisablePasswordlessMode && !_systemInfoPort.IsUserAdministrator())
            return Result.Fail(new Error(ErrorMessageAdminRequiredActivation)
                .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));

        if (!request.DisablePasswordlessMode && _autoLogonPort.IsPasswordlessAuthEnabled())
            return Result.Fail(new Error(ErrorMessageWindowsHelloBlockActive)
                .WithMetadata(StatusKey, AutoLogonResultStatus.WindowsHelloBlockActive));

        if (string.IsNullOrWhiteSpace(request.Username) || request.Password is null)
            return Result.Fail(new Error(ErrorMessageCredentialsMissing)
                .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));

        string domain = request.Domain ?? string.Empty;

        if (!_credentialValidationPort.ValidateCredentials(request.Username, domain, request.Password))
            return Result.Fail(new Error(ErrorMessageInvalidCredentials)
                .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));

        return ApplyAutoLogonActivation(request, domain);
    }

    /// <summary>
    /// Performs the actual system changes once every precondition has been validated, rolling the
    /// passwordless setting back if enabling AutoLogon fails.
    /// </summary>
    private Result<AutoLogonResultStatus> ApplyAutoLogonActivation(ConfigureAutoLogonRequest request, string domain)
    {
        bool passwordlessWasDisabledByUs = false;

        try
        {
            if (request.DisablePasswordlessMode)
            {
                _autoLogonPort.SetPasswordlessAuth(false);
                passwordlessWasDisabledByUs = true;
            }

            if (_autoLogonPort.IsPasswordlessAuthEnabled())
            {
                RestorePasswordlessMode(passwordlessWasDisabledByUs);

                return Result.Fail(new Error(ErrorMessageWindowsHelloBlockActive)
                    .WithMetadata(StatusKey, AutoLogonResultStatus.WindowsHelloBlockActive));
            }

            _autoLogonPort.EnableAutoLogon(request.Username!, domain, request.Password!);

            return Result.Ok(AutoLogonResultStatus.Activated);
        }
        catch
        {
            RestorePasswordlessMode(passwordlessWasDisabledByUs);
            throw;
        }
    }

    /// <summary>
    /// Re-enables passwordless authentication if this operation was the one that disabled it.
    /// </summary>
    private void RestorePasswordlessMode(bool passwordlessWasDisabledByUs)
    {
        if (!passwordlessWasDisabledByUs)
            return;

        try
        {
            _autoLogonPort.SetPasswordlessAuth(true);
        }
        catch (Exception rollbackException)
        {
            _logger.LogError(
                LogEventIds.Security.PasswordlessRollbackFailed,
                rollbackException,
                "AutoLogon rollback failed; passwordless sign-in remains disabled on this machine.");
        }
    }
}
