using System;
using System.IO;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Provider) for deploying and locating browser extensions in Windows AppData.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Deploys and copies browser extension files to local AppData in the Infrastructure layer.<br/>
/// - <strong>Implemented Ports:</strong> <see cref="IOutboundPortBrowserExtensionDeployment"/> and <see cref="IOutboundPortBrowserExtensionPathProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsBrowserExtensionDeploymentProvider : IOutboundPortBrowserExtensionDeployment, IOutboundPortBrowserExtensionPathProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
#if !DEBUG
    private const string AllDirectoriesSearchPattern = "*";
    private const string AllFilesSearchPattern = "*.*";
#endif
    private const string DebugSolutionRelativePath = @"..\..\..\..\..\..\";
    private const string ExtensionFolderName = "TabRestarterExtension";
    private const string ExtensionProjectFolderName = "eBRestarter.TabRestarterExtension";


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Ensures the TabRestarter extension directory is deployed to user AppData in Release mode.
    /// </summary>
    public void EnsureExtensionIsDeployed()
    {
#if !DEBUG
        string targetPath = RetrieveExtensionFolderPath();
        // The source folder which resides as read-only within the installation base directory:
        string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExtensionFolderName);

        // If the extension is already deployed or source folder is missing, skip copying!
        if (Directory.Exists(targetPath) || !Directory.Exists(sourcePath))
        {
            return;
        }

        Directory.CreateDirectory(targetPath);
        CopyDirectoryContents(sourcePath, targetPath);
#endif
    }

    /// <summary>
    /// Retrieves the absolute local filesystem path to the browser extension folder.
    /// </summary>
    public string RetrieveExtensionFolderPath()
    {
#if DEBUG
        // In Debug mode, navigate 6 directory levels up to reach the solution root directory context.
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebugSolutionRelativePath));
        return Path.Combine(solutionDirectory, ExtensionProjectFolderName, ExtensionFolderName);
#else
        // In Release mode: Target the extension deployment folder within the user's localized AppData directory
        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataDirectory, ExtensionFolderName);
#endif
    }

#if !DEBUG
    /// <summary>
    /// Recursively copies all directories and files from source to target path.
    /// </summary>
    /// <param name="sourceDir">Source directory path.</param>
    /// <param name="targetDir">Target directory path.</param>
    private static void CopyDirectoryContents(string sourceDir, string targetDir)
    {
        foreach (var dirPath in Directory.GetDirectories(sourceDir, AllDirectoriesSearchPattern, SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, dirPath);
            Directory.CreateDirectory(Path.Combine(targetDir, relativePath));
        }

        foreach (var newPath in Directory.GetFiles(sourceDir, AllFilesSearchPattern, SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, newPath);
            File.Copy(newPath, Path.Combine(targetDir, relativePath), overwrite: true);
        }
    }
#endif
}