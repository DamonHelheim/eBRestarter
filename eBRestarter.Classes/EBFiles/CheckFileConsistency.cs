using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.Services;
using Serilog;
using System.Text.RegularExpressions;

namespace eBRestarter.Classes.EBFiles
{
    public sealed class ConsistancyCheck : ISystemPaths, IRegexPattern, IConfigConstants
    {
        private static void CreateDefaultConfig()
        {
            IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            Delete delete = new();
            delete.DeleteSingleFile(ISystemPaths.File_Path_eBRestarter_Settings);
            eBXMLFileService.CreateDefaultConfigFileIfNotExist();
        }

        public static void CheckFileConsistency()
        {
            try
            {
                var lineCountSettings = File.ReadLines(ISystemPaths.File_Path_eBRestarter_Settings).Count();

                IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

                //If the line count higher or lower then from the Settings.ini file, delete the file und create a new default Settings.ini file 
                if (lineCountSettings > 19 ^ lineCountSettings < 19)
                {
                    CreateDefaultConfig();
                }
                else
                {
                    try
                    {
                        string Browser = eBXMLFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);
                        string Theme = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.ThemeTag);
                        string DeleteCookiesAndCache = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag);
                        string RestartComputer = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag).ToString();

                        if (Regex.IsMatch(Browser, IRegexPattern.BrowserSelection_Hyphen)) { }
                        else
                        {
                            eBXMLFileService.WriteXMLAttributeInConfig(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                                                                                                      nameof(Browser),
                                                                                                                                                                      IConfigConstants.BrowserTag_Selected_Attribut, "");
                        }

                        if (Regex.IsMatch(Theme, IRegexPattern.ThemeSelection)) { } else { eBXMLFileService.WriteXMLTagValueInConfig(nameof(Theme), "Light"); }

                        if (Regex.IsMatch(DeleteCookiesAndCache, IRegexPattern.DeleteCookiesAndCacheSelection)) { } else { eBXMLFileService.WriteXMLTagValueInConfig(nameof(DeleteCookiesAndCache), "false"); }
                        if (Regex.IsMatch(RestartComputer, IRegexPattern.RestartComputerSelection)) { } else { eBXMLFileService.WriteXMLTagValueInConfig(nameof(RestartComputer), "false"); }

                        try
                        {
                            int Runtime = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                       IConfigConstants.BrowserRuntimeTag);

                            if (Runtime >= 1 && Runtime <= 12) { } else { eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.BrowserRuntimeTag, "1"); }
                        }
                        catch (Exception e)
                        {

                            Log.Error(e, "Es ist eine Ausnahme in der Konsistenzprüfung aufgetreten. Logging...");
                            eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.BrowserRuntimeTag, "1");
                        }

                        try
                        {
                            int Rest = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                    IConfigConstants.BrowserRestTag);

                            if (Rest >= 20 && Rest <= 60) { } else { eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.BrowserRestTag, "20"); }
                        }
                        catch (Exception e)
                        {

                            Log.Error(e, "Es ist eine Ausnahme in der Konsistenzprüfung aufgetreten. Logging...");
                            eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.BrowserRestTag, "20");
                        }

                        string StartWithWindows = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                                  IConfigConstants.StartWithWindowsTag).ToLower();

                        if (StartWithWindows.Equals("true")) { }
                        else if (StartWithWindows.Equals("false")) { }
                        else { eBXMLFileService.WriteXMLTagValueInConfig(nameof(StartWithWindows), "false"); }


                        string CheckBrowserAliveRoutine = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                                          IConfigConstants.CheckBrowserAliveRoutineTag).ToLower();

                        if (CheckBrowserAliveRoutine.Equals("true")) { }
                        else if (CheckBrowserAliveRoutine.Equals("false")) { }
                        else { eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.CheckBrowserAliveRoutineTag, "false"); }


                        string StartRestarterWithProgrammStart = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                                                 IConfigConstants.StartRestarterWithProgrammStartTag).ToLower();
                        if (StartRestarterWithProgrammStart.Equals("true")) { }
                        else if (StartRestarterWithProgrammStart.Equals("false")) { }
                        else { eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.StartRestarterWithProgrammStartTag, "false"); }

                        try
                        {
                            int RestartTime = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings,
                                                                                           IConfigConstants.BrowserRuntimeTag);

                            if (RestartTime >= 1 && RestartTime <= 23) { } else { eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.BrowserRuntimeTag, "1"); }
                        }
                        catch (Exception e)
                        {

                            Log.Error(e, "Es ist eine Ausnahme in der Konsistenzprüfung aufgetreten. Logging...");
                            eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.BrowserRuntimeTag, "1");
                        }

                        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
                    }

                    catch (Exception e)
                    {
                        Log.Error(e, "Es ist eine Ausnahme in der Konsistenzprüfung aufgetreten. Logging...");

                        CreateDefaultConfig();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme in der Konsistenzprüfung aufgetreten. Logging...");
            }
        }
    }
}