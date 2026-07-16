namespace eBRestarter.Core.Application.ObjectArchetypes.Enums;

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