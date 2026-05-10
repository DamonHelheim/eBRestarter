using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using FluentResults;
using FluentValidation;

namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public class ConfigureAutoLogonService(
    IWindowsAutoLogonService autoLogonService,
    IWindowsSystemInfoService windowsSystemInfoService,
    ICredentialValidationService credentialValidationService,
    IValidator<ConfigureAutoLogonRequest> validator) : IConfigureAutoLogonUseCase
{
    private readonly IWindowsAutoLogonService _autoLogonService = autoLogonService;
    private readonly IWindowsSystemInfoService _windowsSystemInfoService = windowsSystemInfoService;
    private readonly ICredentialValidationService _credentialValidationService = credentialValidationService;
    private readonly IValidator<ConfigureAutoLogonRequest> _validator = validator;

    public Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request)
    {
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result.Fail(new Error("Validation failed.")
                .WithMetadata("Status", AutoLogonResultStatus.ValidationError));
        }

        try
        {
            if (request.IsDeactivateAction)
            {
                if (request.RestorePasswordlessMode)
                {
                    if (!_windowsSystemInfoService.IsUserAdministrator())
                    {
                        return Result.Fail(new Error("Administrator rights required to restore passwordless mode.")
                            .WithMetadata("Status", AutoLogonResultStatus.AdminRequired));
                    }
                    _autoLogonService.SetWindowsHelloPasswordlessState(true);
                }

                _autoLogonService.DisableAutoLogon();
                return Result.Ok(AutoLogonResultStatus.Deactivated);
            }

            if (request.DisablePasswordlessMode)
            {
                if (!_windowsSystemInfoService.IsUserAdministrator())
                {
                    return Result.Fail(new Error("Administrator rights required to disable passwordless mode.")
                        .WithMetadata("Status", AutoLogonResultStatus.AdminRequired));
                }
                _autoLogonService.SetWindowsHelloPasswordlessState(false);
            }

            if (_autoLogonService.IsWindowsHelloPasswordlessEnabled())
            {
                return Result.Fail(new Error("Windows Hello Passwordless Mode ist aktiv. AutoLogon nicht möglich.")
                    .WithMetadata("Status", AutoLogonResultStatus.WindowsHelloBlockActive));
            }

            if (!string.IsNullOrWhiteSpace(request.Username) && request.Password != null)
            {
                string domain = request.Domain ?? string.Empty;

                bool isValid = _credentialValidationService.ValidateCredentials(request.Username, domain, request.Password);

                if (!isValid)
                {
                    return Result.Fail(new Error("Invalid credentials.")
                        .WithMetadata("Status", AutoLogonResultStatus.ValidationError));
                }

                _autoLogonService.EnableAutoLogon(request.Username, domain, request.Password);

                return Result.Ok(AutoLogonResultStatus.Activated);
            }

            return Result.Fail(new Error("Credentials missing.")
                .WithMetadata("Status", AutoLogonResultStatus.ValidationError));
        }
        catch (InvalidOperationException)
        {
            return Result.Fail(new Error("Domain error.")
                .WithMetadata("Status", AutoLogonResultStatus.DomainError));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex)
                .WithMetadata("Status", AutoLogonResultStatus.UnexpectedError));
        }
    }
}
