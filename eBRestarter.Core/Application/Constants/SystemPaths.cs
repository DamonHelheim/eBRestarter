namespace eBRestarter.Core.Application.Constants;

public static class SystemPaths
{
    // Hersteller/App Name für die Ordnerstruktur
    private const string ManufacturerName = "Skylar";
    private const string AppName = "eBRestarter";

    // Der Basis-Ordner deiner App: ...\AppData\Local\Skylar\eBRestarter\
    public static string ApplicationDataBasePath => Path.Combine(LocalAppData, ManufacturerName, AppName);

    // Der Download-Ordner: ...\Users\User\Downloads\
    public static string DownloadsPath => Path.Combine(UserProfile, "Downloads");

    public static string FilenameFirefoxInstaller => Path.Combine(DownloadsPath, "Firefox installer x64.exe");
    public static string FilenameChromeInstaller => Path.Combine(DownloadsPath, "ChromeSetup x64.exe");
    public static string FilenameEdgeInstaller => Path.Combine(DownloadsPath, "MicrosoftEdgeEnterpriseX64.msi");

    public static string File_Path_eBRestarter_Settings => Path.Combine(ApplicationDataBasePath, "eBRestarterConfig.json");
    public static string File_Path_Time_Stamp_Delete_Process => Path.Combine(ApplicationDataBasePath, "DeleteDate.ini");
    public static string File_Path_Time_Stamp_Restart_Process => Path.Combine(ApplicationDataBasePath, "RestartDate.ini");
    public static string File_Path_API => Path.Combine(ApplicationDataBasePath, "EB_API_File.apiaf");
    public static string File_Path_Logging => Path.Combine(ApplicationDataBasePath, "log.txt");

    // Gibt dynamisch den korrekten Pfad zu "AppData/Local" zurück (z.B. C:\Users\User\AppData\Local)
    // Das ist viel sicherer als manuelles String-Basteln.
    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    // Gibt dynamisch den User-Profil-Ordner zurück (z.B. C:\Users\User)
    private static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);



    // Helper: Stellt sicher, dass der App-Ordner existiert (kann beim Start aufgerufen werden)
    public static void EnsureApplicationDirectoryExists()
    {
        if (!Directory.Exists(ApplicationDataBasePath))
        {
            Directory.CreateDirectory(ApplicationDataBasePath);
        }
    }
}
