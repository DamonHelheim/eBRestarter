using eBRestarter.Core.Application.Ports.Inbound.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using FluentResults;
using FluentValidation;


namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public sealed class ConfigureAutoLogonUseCase(
    IOsAutoLogonPort autoLogonService,
    ISystemInfoPort WindowsSystemInfoAdapter,
    ICredentialValidationPort credentialValidationUseCase,
    IValidator<ConfigureAutoLogonRequest> validator) : IConfigureAutoLogonUseCase
{
    private const string StatusKey = "Status";

    private readonly IOsAutoLogonPort _autoLogonService = autoLogonService;
    private readonly ISystemInfoPort _windowsSystemInfoService = WindowsSystemInfoAdapter;
    private readonly ICredentialValidationPort _credentialValidationUseCase = credentialValidationUseCase;
    private readonly IValidator<ConfigureAutoLogonRequest> _validator = validator;


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
            if (!_windowsSystemInfoService.IsUserAdministrator())
            {
                return Result.Fail(new Error("Administrator rights required to restore passwordless mode.")
                    .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));
            }
            _autoLogonService.SetPasswordlessAuth(true);
        }

        _autoLogonService.DisableAutoLogon();
        return Result.Ok(AutoLogonResultStatus.Deactivated);
    }

    private Result<AutoLogonResultStatus> HandleActivation(ConfigureAutoLogonRequest request)
    {
        if (request.DisablePasswordlessMode)
        {
            if (!_windowsSystemInfoService.IsUserAdministrator())
            {
                return Result.Fail(new Error("Administrator rights required to disable passwordless mode.")
                    .WithMetadata(StatusKey, AutoLogonResultStatus.AdminRequired));
            }
            _autoLogonService.SetPasswordlessAuth(false);
        }

        if (_autoLogonService.IsPasswordlessAuthEnabled())
        {
            return Result.Fail(new Error("Windows Hello Passwordless Mode ist aktiv. AutoLogon nicht mÃ¶glich.")
                .WithMetadata(StatusKey, AutoLogonResultStatus.WindowsHelloBlockActive));
        }

        if (!string.IsNullOrWhiteSpace(request.Username) && request.Password is not null)
        {
            string domain = request.Domain ?? string.Empty;

            var isValid = _credentialValidationUseCase.ValidateCredentials(request.Username, domain, request.Password);

            if (!isValid)
            {
                return Result.Fail(new Error("Invalid credentials.")
                    .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
            }

            _autoLogonService.EnableAutoLogon(request.Username, domain, request.Password);

            return Result.Ok(AutoLogonResultStatus.Activated);
        }

        return Result.Fail(new Error("Credentials missing.")
            .WithMetadata(StatusKey, AutoLogonResultStatus.ValidationError));
    }
}








