using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services;

public class BrowserExtensionDeploymentService : IBrowserExtensionDeploymentService
{
    public void EnsureExtensionIsDeployed()
    {
#if !DEBUG
        string targetPath = RetrieveExtensionFolderPath();
        // Der Ursprungsordner, der read-only im Installationsverzeichnis liegt:
        string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TabRestarterExtension");

        // Wenn die Erweiterung noch nicht im AppData liegt, kopieren wir sie!
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
        // Im Debug-Modus gehen wir 6 Ebenen nach oben in den Solution-Root-Ordner.
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\..\"));
        return Path.Combine(solutionDirectory, "eBRestarter.TabRestarterExtension", "TabRestarterExtension");
#else
        // Im Release-Modus: Extension im AppData-Ordner des Users
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
            File.Copy(newPath, newPath.Replace(sourceDir, targetDir), true);
        }
    }
}
