namespace eBRestarter.Infrastructure.OperatingSystem;

public static class SystemPaths
{
    // Manufacturer/App name for the folder structure
    private const string ManufacturerName = "Skylar";
    private const string AppName = "eBRestarter";

    // The base folder of your app: ...\AppData\Local\Skylar\eBRestarter\
    public static string ApplicationDataBasePath => Path.Combine(LocalAppData, ManufacturerName, AppName);

    // The download folder: ...\Users\User\Downloads\
    public static string DownloadsPath => Path.Combine(UserProfile, "Downloads");

    public static string FilenameFirefoxInstaller => Path.Combine(DownloadsPath, "Firefox installer x64.exe");
    public static string FilenameChromeInstaller => Path.Combine(DownloadsPath, "ChromeSetup x64.exe");
    public static string FilenameEdgeInstaller => Path.Combine(DownloadsPath, "MicrosoftEdgeEnterpriseX64.msi");

    public static string File_Path_eBRestarter_Settings => Path.Combine(ApplicationDataBasePath, "eBRestarterConfig.json");
    public static string File_Path_Time_Stamp_Delete_Process => Path.Combine(ApplicationDataBasePath, "DeleteDate.ini");
    public static string File_Path_Time_Stamp_Restart_Process => Path.Combine(ApplicationDataBasePath, "RestartDate.ini");
    public static string File_Path_API => Path.Combine(ApplicationDataBasePath, "EB_API_File.apiaf");
    public static string File_Path_Logging => Path.Combine(ApplicationDataBasePath, "log.txt");

    // Dynamically returns the correct path to "AppData/Local" (e.g., C:\Users\User\AppData\Local)
    // This is much safer than manual string concatenation.
    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    // Dynamically returns the user profile folder (e.g., C:\Users\User)
    private static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>
    /// Helper: Ensures that the application directory exists (can be called at startup).
    /// </summary>
    public static void EnsureApplicationDirectoryExists()
    {
        if (!Directory.Exists(ApplicationDataBasePath))
        {
            Directory.CreateDirectory(ApplicationDataBasePath);
        }
    }
}

