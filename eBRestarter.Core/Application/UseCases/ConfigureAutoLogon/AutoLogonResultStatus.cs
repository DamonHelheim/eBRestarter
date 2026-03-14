namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public enum AutoLogonResultStatus
{
    Activated,
    Deactivated,
    ValidationError,
    DomainError,
    UnexpectedError
}