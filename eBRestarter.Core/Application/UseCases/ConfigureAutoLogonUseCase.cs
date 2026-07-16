using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.UseCases;

public sealed class ConfigureAutoLogonUseCase(
    IOutboundPortOsAutoLogonRepository autoLogonPort,
    IOutboundPortSystemInfoProvider systemInfoPort,
    IOutboundPortCredentialValidationProvider credentialValidationPort,
    IInboundPortApplicationValidator<ConfigureAutoLogonRequest> validator) : IUseCaseConfigureAutoLogon
{
    private const string StatusKey = "Status";

    private readonly IOutboundPortOsAutoLogonRepository _autoLogonPort = autoLogonPort;
    private readonly IOutboundPortSystemInfoProvider _systemInfoPort = systemInfoPort;
    private readonly IOutboundPortCredentialValidationProvider _credentialValidationPort = credentialValidationPort;
    private readonly IInboundPortApplicationValidator<ConfigureAutoLogonRequest> _validator = validator;

    public Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Fail(new Error("Validation failed.")
                .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
        }

        try
        {
            if (request.IsDeactivateAction)
            {
                return HandleDeactivation(request);
            }

            return HandleActivation(request);
        }
        catch (InvalidOperationException)
        {
            return Result.Fail(new Error("Domain error.")
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
                return Result.Fail(new Error("Administrator rights required to restore passwordless mode.")
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
                return Result.Fail(new Error("Administrator rights required to disable passwordless mode.")
                    .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));
            }
            _autoLogonPort.SetPasswordlessAuth(false);
        }

        if (_autoLogonPort.IsPasswordlessAuthEnabled())
        {
            return Result.Fail(new Error("Windows Hello Passwordless Mode ist aktiv. AutoLogon nicht mÃ¶glich.")
                .WithMetadata(StatusKey, AutoLogonResultStatus.WindowsHelloBlockActive));
        }

        if (!string.IsNullOrWhiteSpace(request.Username) && request.Password is not null)
        {
            string domain = request.Domain ?? string.Empty;

            var isValid = _credentialValidationPort.ValidateCredentials(request.Username, domain, request.Password);

            if (!isValid)
            {
                return Result.Fail(new Error("Invalid credentials.")
                    .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
            }

            _autoLogonPort.EnableAutoLogon(request.Username, domain, request.Password);

            return Result.Ok(AutoLogonResultStatus.Activated);
        }

        return Result.Fail(new Error("Credentials missing.")
            .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
    }
}








