using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public class ConfigureAutoLogonService(
    IWindowsAutoLogonService autoLogonService,
    ICredentialValidationService credentialValidationService) : IConfigureAutoLogonUseCase
{
    private readonly IWindowsAutoLogonService _autoLogonService = autoLogonService;
    private readonly ICredentialValidationService _credentialValidationService = credentialValidationService;

    public ConfigureAutoLogonResponse Execute(ConfigureAutoLogonRequest request)
    {
        try
        {
            if (request.IsDeactivateAction)
            {
                _autoLogonService.DisableAutoLogon();

                return new ConfigureAutoLogonResponse(true, AutoLogonResultStatus.Deactivated);
            }

            // --- NEU: Den Windows Hello Check durchführen ---
            if (_autoLogonService.IsWindowsHelloPasswordlessEnabled())
            {
                // Wenn aktiv, brechen wir sofort ab!
                return new ConfigureAutoLogonResponse(false, AutoLogonResultStatus.WindowsHelloBlockActive,
                    "Windows Hello Passwordless Mode ist aktiv. AutoLogon nicht möglich.");
            }
            // ------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.Username) && request.Password != null)
            {
                string domain = request.Domain ?? string.Empty;

                bool isValid = _credentialValidationService.ValidateCredentials(request.Username, domain, request.Password);

                if (!isValid)
                {
                    return new ConfigureAutoLogonResponse(false, AutoLogonResultStatus.ValidationError);
                }

                _autoLogonService.EnableAutoLogon(request.Username, domain, request.Password);

                return new ConfigureAutoLogonResponse(true, AutoLogonResultStatus.Activated);
            }

            return new ConfigureAutoLogonResponse(false, AutoLogonResultStatus.ValidationError, "Credentials missing.");
        }
        catch (InvalidOperationException)
        {
            return new ConfigureAutoLogonResponse(false, AutoLogonResultStatus.DomainError);
        }
        catch (Exception ex)
        {
            return new ConfigureAutoLogonResponse(false, AutoLogonResultStatus.UnexpectedError, ex.Message);
        }
    }
}