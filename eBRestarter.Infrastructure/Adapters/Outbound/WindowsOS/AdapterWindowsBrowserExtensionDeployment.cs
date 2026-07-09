using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Provider) for deploying and locating browser extensions in Windows AppData.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Kopie und Bereitstellung von Browser-Erweiterungen im lokalen Dateisystem.<br/>
/// - <strong>Implementierte Ports:</strong> <see cref="IOutboundPortBrowserExtensionDeployment"/> und <see cref="IOutboundPortBrowserExtensionPathProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, Outbound Ports implementiert und vom Core angetrieben wird, um Datei-Installationsschritte auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsBrowserExtensionDeployment : IOutboundPortBrowserExtensionDeployment, IOutboundPortBrowserExtensionPathProvider
{
    public void EnsureExtensionIsDeployed()
    {
#if !DEBUG
        string targetPath = RetrieveExtensionFolderPath();
        // The source folder which resides as read-only within the installation base directory:
        string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TabRestarterExtension");

        // If the extension is not yet deployed to AppData, copy it over!
        if (!Directory.Exists(targetPath) && Directory.Exists(sourcePath))
        {
            Directory.CreateDirectory(targetPath);
            CopyDirectoryContents(sourcePath, targetPath);
        }
#endif
    }

    public string RetrieveExtensionFolderPath()
    {
#if DEBUG
        // In Debug mode, navigate 6 directory levels up to reach the solution root directory context.
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\..\"));
        return Path.Combine(solutionDirectory, "eBRestarter.TabRestarterExtension", "TabRestarterExtension");
#else
        // In Release mode: Target the extension deployment folder within the user's localized AppData directory
        return Path.Combine(SystemPaths.ApplicationDataBasePath, "TabRestarterExtension");
#endif
    }

#pragma warning disable RCS1213 // Remove unused member declaration
    private static void CopyDirectoryContents(string sourceDir, string targetDir)
#pragma warning restore RCS1213 // Remove unused member declaration
    {
        foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
        }

        foreach (var newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourceDir, targetDir), overwrite: true);
        }
    }
}