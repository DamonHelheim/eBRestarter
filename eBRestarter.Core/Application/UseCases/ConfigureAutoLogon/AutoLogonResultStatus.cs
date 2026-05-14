namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public enum AutoLogonResultStatus
{
    Activated,
    Deactivated,
    WindowsHelloBlockActive,
    ValidationError,
    DomainError,
    UnexpectedError,
    AdminRequired
}
