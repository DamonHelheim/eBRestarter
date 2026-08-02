using System;
using System.IO;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Provider) for deploying and locating browser extensions in Windows AppData.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Kopie und Bereitstellung von Browser-Erweiterungen im lokalen Dateisystem.<br/>
/// - <strong>Implementierte Ports:</strong> <see cref="IOutboundPortBrowserExtensionDeployment"/> und <see cref="IOutboundPortBrowserExtensionPathProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, Outbound Ports implementiert und vom Core angetrieben wird, um Datei-Installationsschritte auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsBrowserExtensionDeploymentProvider : IOutboundPortBrowserExtensionDeployment, IOutboundPortBrowserExtensionPathProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
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