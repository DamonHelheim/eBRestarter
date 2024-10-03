using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using Serilog;
using System.Text.RegularExpressions;

namespace eBRestarter.Classes.Services
{
    public sealed class DateStampService : ISystemPaths
    {
        
        private readonly IEBFile eBFileManagement = new EBFileManagement();
        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
        public static string GetFutureDate(int days)
        {
            DateTime thisDay = DateTime.Today;

            return thisDay.AddDays(days).ToString("d");
        }

        public static string GetDateNow()
        {
            DateTime thisDay = DateTime.Today;

            return thisDay.ToString("d");
        }

        public bool ReachDateDeleteProcess()
        {
            try
            {
                EBFileService eBFileService = new(eBFileManagement);

                EBFileAccess.AccessFile!(IFileNames.DELETEDATE);

                string getDateInNextDays = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process);

                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

                if (eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process).Equals("false"))
                {
                    return false;

                }
                else
                {

                    int daysOfNextDelete = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag);

                    if (GetDateNow().Equals(getDateInNextDays))
                    {
                        //Write the next delete Date in File
                        eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, GetFutureDate(daysOfNextDelete));

                        EBFileAccess.LockFile!(IFileNames.DELETEDATE);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im DateStampServicebereich aufgetreten. Logging...");
                return false;
            }
        }

        public void DateIsLessThanTodayForDeletionProcess()
        {
            try
            {
                EBFileService eBFileService = new(eBFileManagement);
                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

                string getDateInNextDays = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process);

                //Debug.WriteLine(getDateInNextDays);
                if (getDateInNextDays.Contains("false"))
                {

                }
                else
                {
                    if (!Regex.IsMatch(getDateInNextDays, @"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$"))
                    {
                        eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, "false");

                    }
                    else if (DateTime.Parse(getDateInNextDays) <= DateTime.Parse(GetDateNow()))
                    {
                        int daysOfNextDelete = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag);
                        eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, GetFutureDate(daysOfNextDelete));
                    }

                    AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
                }

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im DateStampServicebereich aufgetreten. Logging...");
            }
        }

        public void DateIsLessThanTodayForRestartProcess()
        {
            try
            {
                EBFileService eBFileService = new(eBFileManagement);
                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

                string getDateInNextDays = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process);

                if (getDateInNextDays.Contains("false"))
                {
                    //Do nothing
                }
                else
                {
                    if (!Regex.IsMatch(getDateInNextDays, @"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$"))
                    {
                        eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process, "false");

                    }
                    else if (DateTime.Parse(getDateInNextDays) <= DateTime.Parse(GetDateNow()))
                    {
                        int daysOfNextDelete = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag);
                        eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process, GetFutureDate(daysOfNextDelete));
                    }

                    AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im DateStampServicebereich aufgetreten. Logging...");
            }
        }
    }
}
