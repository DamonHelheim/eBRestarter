namespace eBRestarter.Classes.EBInterfaces
{
    public interface ISystemPaths
    {
        /*
         * Contains all filepaths from the application
         */

        private const string USERS = @"Users\";
        private const string APP_DATA_PATH = "\\AppData\\Local\\Skylar\\eBRestarter\\";

        public static string DownloadFolderPath { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + "\\Downloads\\"; }

        public static string FilenameFirefoxInstaller { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + "\\Downloads\\Firefox installer x64.exe"; }
        public static string FilenameChromeInstaller { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + "\\Downloads\\ChromeSetup x64.exe"; }
        public static string FilenameEdgeInstaller { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + "\\Downloads\\MicrosoftEdgeEnterpriseX64.msi"; }

        public static string FilePathApplicationOnly { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + APP_DATA_PATH; }

        public static string File_Path_eBRestarter_Settings { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + APP_DATA_PATH + "eBRestarterConfig.xml"; }
        public static string File_Path_Time_Stamp_Delete_Process { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + APP_DATA_PATH + "DeleteDate.ini"; }
        public static string File_Path_Time_Stamp_Restart_Process { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + APP_DATA_PATH + "RestartDate.ini"; }
        public static string File_Path_API { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + APP_DATA_PATH + "EB_API_File.apiaf"; }

        public static string File_Path_Logging { get => @Path.GetPathRoot(Environment.SystemDirectory) + USERS + Environment.UserName + APP_DATA_PATH + "log.txt"; }

    }
}
