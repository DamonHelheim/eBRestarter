namespace eBRestarter.Core.Application.Enums;

/// <summary>
/// Specifies the scope/target of a directory credential validation check, decoupling Application Core from System.DirectoryServices.
/// </summary>
public enum DirectoryContextScope
{
    Machine,
    Domain,
    ApplicationDirectory
}
