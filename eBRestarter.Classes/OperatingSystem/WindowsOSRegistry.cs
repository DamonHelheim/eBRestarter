using Microsoft.Win32;
using Serilog;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace eBRestarter.Classes.OperatingSystem
{
    public partial class WindowsOS
    {
        //Classes variables
        private const string regex_Path_1_Standard_Browser = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
        private const string regex_Path_2_Standard_Browser = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice";

        private const string startUp_Registry_Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string passwordLess_Registry_Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";
        private const string passwordLess_Registry_Full_Path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

        private const string edgeStartupBoostEnabled_Registry_Path = @"SOFTWARE\Policies\Microsoft\Edge";

        #pragma warning disable CA1822
        [SupportedOSPlatform("windows")]
        public string? GetCurrentBrowserVersion(string regPath, string firstRegArguments, string secondRegArgument)
        {
            if (Registry.GetValue(regPath, firstRegArguments, secondRegArgument) != null)
            {
                return Registry.GetValue(regPath, firstRegArguments, secondRegArgument) as string;
            }
            else
            {
                return null;
            }
        }

        [SupportedOSPlatform("windows")]
        public string StandardBrowser()
        {
            string? windowsStandardBrowser = Regex.Replace(GetCurrentBrowserVersion(regex_Path_1_Standard_Browser, "ProgId", "")!, "", "");
            string? windowsStandardBrowser2 = Regex.Replace(GetCurrentBrowserVersion(regex_Path_2_Standard_Browser, "ProgId", "")!, "", "");

            string regexMatch = "(FirefoxURL-308046B0AF4A39CB|ChromeHTML)";

            string wsb1 = Regex.Match(windowsStandardBrowser, regexMatch).ToString();
            string wsb2 = Regex.Match(windowsStandardBrowser2, regexMatch).ToString();

            if ((string.Equals(wsb1, wsb2) && Regex.IsMatch(wsb1, "FirefoxURL-308046B0AF4A39CB") && Regex.IsMatch(wsb2, "FirefoxURL-308046B0AF4A39CB")))
            {
                AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
                return "Firefox";
            }
            else if (string.Equals(wsb1, wsb2) && Regex.IsMatch(wsb1, "ChromeHTML") && Regex.IsMatch(wsb2, "ChromeHTML"))
            {
                AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
                return "Chrome";
            }
            else
            {
                return "-";
            }
        }


        //Sets an entry in the registry that ensures that the application starts by itself the next time the operating system is started.
        [SupportedOSPlatform("windows")]
        public void SetStartUp()
        {
            try
            {
                using (RegistryKey? rkApp = Registry.CurrentUser.OpenSubKey(startUp_Registry_Path, true))
                {
                    rkApp!.SetValue("eB Restarter", Process.GetCurrentProcess().MainModule!.FileName);
                    rkApp.Close();
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
            }
        }

        [SupportedOSPlatform("windows")]
        public Dictionary<string, object> GetRegistrySubKeysStartUp()
        {
            try
            {
                var valuesBynames = new Dictionary<string, object>();
                //Here I'm looking under LocalMachine. You can replace it with Registry.CurrentUser for current user...
                using (RegistryKey rootKey = Registry.CurrentUser.OpenSubKey(startUp_Registry_Path)!)
                {
                    if (rootKey != null)
                    {
                        string[] valueNames = rootKey.GetValueNames();
                        foreach (string currSubKey in valueNames)
                        {
                            object value = rootKey.GetValue(currSubKey)!;
                            valuesBynames.Add(currSubKey, value);
                        }

                        rootKey.Close();
                    }
                }
                return valuesBynames;

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
                return null!;
            }


        }

        //Remove start up with windows
        [SupportedOSPlatform("windows")]
        public void DeleteStartUp()
        {
            try
            {

                using (RegistryKey? rkApp = Registry.CurrentUser.OpenSubKey(startUp_Registry_Path, true))
                {
                    rkApp!.DeleteValue("eB Restarter", false);
                    rkApp.Close();
                };

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
            }
        }

        [SupportedOSPlatform("windows")]
        public void ActivatePossibilityUserAutologonForWindows()
        {
            try
            {
                using (RegistryKey? myKey = Registry.LocalMachine.OpenSubKey(passwordLess_Registry_Path, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (myKey != null)
                    {
                        myKey.SetValue("DevicePasswordLessBuildVersion", "0", RegistryValueKind.DWord);
                        myKey.Close();

                    }
                    else
                    {
                        RegistryKey myKey2 = Registry.LocalMachine.CreateSubKey(passwordLess_Registry_Path);
                        myKey2.SetValue("DevicePasswordLessBuildVersion", "0", RegistryValueKind.DWord);
                        myKey2.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
            }

        }

        //edgeStartupBoostEnabled_Registry_Path //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft
        [SupportedOSPlatform("windows")]
        public void DeactivateStartupBoostEdge()
        {
            using (RegistryKey? myKey = Registry.LocalMachine.OpenSubKey(edgeStartupBoostEnabled_Registry_Path, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (myKey != null)
                {
                    myKey.SetValue("StartupBoostEnabled", "0", RegistryValueKind.DWord);
                    myKey.Close();
                }
                else
                {
                    using (RegistryKey myKey2 = Registry.LocalMachine.CreateSubKey(edgeStartupBoostEnabled_Registry_Path))
                    {
                        myKey2.SetValue("StartupBoostEnabled", "0", RegistryValueKind.DWord);
                        myKey2.Close();
                    }
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public void ActivateStartupBoostEdge()
        {
            RegistryKey? myKey = Registry.LocalMachine.OpenSubKey(edgeStartupBoostEnabled_Registry_Path, RegistryKeyPermissionCheck.ReadWriteSubTree);

            if (myKey != null)
            {
                myKey.SetValue("StartupBoostEnabled", "1", RegistryValueKind.DWord);
                myKey.Close();
            }
            else
            {
                RegistryKey myKey2 = Registry.LocalMachine.CreateSubKey(edgeStartupBoostEnabled_Registry_Path);
                myKey2.SetValue("StartupBoostEnabled", "1", RegistryValueKind.DWord);
                myKey2.Close();
            }
        }

        [SupportedOSPlatform("windows")]
        public void DeactivatePossibilityUserAutologonForWindows()
        {
            try
            {
                using (RegistryKey? myKey = Registry.LocalMachine.OpenSubKey(passwordLess_Registry_Path, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (myKey != null)
                    {
                        myKey.SetValue("DevicePasswordLessBuildVersion", "2", RegistryValueKind.DWord);
                        myKey.Close();

                        RegistryKey myKey2 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                        myKey2.DeleteSubKeyTree(@"PasswordLess");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im WindowsOSbereich aufgetreten. Logging...");
            }
        }


        [SupportedOSPlatform("windows")]
        public bool CheckPossibilityUserAutologonForWindowsIsActivated()
        {
            if (Registry.GetValue(passwordLess_Registry_Full_Path, "DevicePasswordLessBuildVersion", "") != null)
            {
                string? checkflag = ((int)Registry.GetValue(passwordLess_Registry_Full_Path, "DevicePasswordLessBuildVersion", "")!).ToString();

                if (checkflag.Equals("0"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
