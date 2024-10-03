using Microsoft.Win32;
using Serilog;
using System.Runtime.Versioning;

namespace eBRestarter.Classes.OperatingSystem
{    
    public partial class WindowsOS
    {  
        [SupportedOSPlatform("windows")]
        private readonly RegistryKey registryKey = null!;

        [SupportedOSPlatform("windows")]
        public WindowsOS() {

            try
            {
                registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")!;

            } catch(Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
            }      
        }


        [SupportedOSPlatform("windows")]
        public string GetCurrentOSBuildVersion()
        {
            return Environment.OSVersion.Version.Build.ToString() + "." + registryKey.GetValue("UBR")!.ToString(); ;
        }

        [SupportedOSPlatform("windows")]
        public static string GetCurrentOSVersion()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion", "")!.ToString()! ;
        }        

    }
}
