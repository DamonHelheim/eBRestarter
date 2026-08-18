namespace eBRestarter.Core.Application.ObjectArchetypes.Enums;

/// <summary>
/// Specifies the scope/target of a directory credential validation check, decoupling Application Core from System.DirectoryServices.
/// </summary>
public enum DirectoryContextScope
{
    /// <summary>
    /// Validates credentials against the local machine account store.
    /// </summary>
    Machine,

    /// <summary>
    /// Validates credentials against an Active Directory domain store.
    /// </summary>
    Domain,

    /// <summary>
    /// Validates credentials against the application directory context.
    /// </summary>
    ApplicationDirectory
}
