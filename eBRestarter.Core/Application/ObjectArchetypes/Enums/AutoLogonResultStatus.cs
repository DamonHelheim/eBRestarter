namespace eBRestarter.Core.Application.ObjectArchetypes.Enums;

/// <summary>
/// Represents the result status of Windows AutoLogon configuration operations.
/// </summary>
public enum AutoLogonResultStatus
{
    /// <summary>
    /// AutoLogon has been successfully activated in the registry.
    /// </summary>
    Activated,

    /// <summary>
    /// AutoLogon has been successfully deactivated in the registry.
    /// </summary>
    Deactivated,

    /// <summary>
    /// Operation failed because Windows Hello passwordless sign-in mode is active.
    /// </summary>
    WindowsHelloBlockActive,

    /// <summary>
    /// Operation failed due to invalid credentials or parameters.
    /// </summary>
    ValidationError,

    /// <summary>
    /// Operation failed due to domain validation or machine account errors.
    /// </summary>
    DomainError,

    /// <summary>
    /// Operation failed due to an unexpected system or registry error.
    /// </summary>
    UnexpectedError,

    /// <summary>
    /// Operation failed because elevated administrator privileges are required.
    /// </summary>
    AdminRequired
}