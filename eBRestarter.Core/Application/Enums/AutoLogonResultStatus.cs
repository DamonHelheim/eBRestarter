namespace eBRestarter.Core.Application.Enums;

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

