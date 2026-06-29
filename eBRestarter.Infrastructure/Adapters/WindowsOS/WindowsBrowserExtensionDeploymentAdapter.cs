using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Formatters;
using eBRestarter.Core.Application.Ports.Outbound;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

public sealed class WindowsBrowserExtensionDeploymentAdapter : IBrowserExtensionDeploymentPort
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