using eBRestarter.Classes.Cache;
using eBRestarter.Classes.OperatingSystem;
using System.Diagnostics;

namespace eBRestarter.Classes.EBTask
{
    public sealed class EBTask
    {

        private readonly WindowsOS windows = new();

        private bool _restartTrigger = false;

        //Task for restart the machine
        public void RestartClient()
        {
            ThreadStart @start = RestartClientTask;

            var RestartClientThread = new Thread(@start)
            {
                Name = "Restart Client",
                IsBackground = true
            };

            RestartClientThread.Start();
        }

        private void RestartClientTask()
        {
            while (true)
            {

                string restartComputerSetting = SettingsCache.Position["RestartDate"];

                //Debug.WriteLine(restartComputerSetting);

                //if this is set to false, then ignore the following processes below it
                if (!restartComputerSetting.Equals("false"))
                {
                    //Important variable for the while loop below
                    _restartTrigger = false;

                    //While restartTrigger is false, keep in this loop
                    while (_restartTrigger == false)
                    {

                        Thread.Sleep(1000);

                        string dateOfRestartFile = SettingsCache.Position["RestartDate"];

                        string restartComputerSetting2 = SettingsCache.Position["RestartComputer"];

                        //Debug.WriteLine("Warte auf Restart " + dateOfRestartFile + " " + DateTime.Today.ToShortDateString() + " " + dateOfRestartFile.Equals(DateTime.Today.ToShortDateString()));

                        if (restartComputerSetting2.Equals("false") || dateOfRestartFile.Equals("false"))
                        {
                            //Serves the terminate this sections of this loop, if the user set the restart settings to false
                            _restartTrigger = true;
                        }

                        //If date of restart is equals like in the saved restart stamp setting and the restartTrigger is false, then keep in this loop
                        while (dateOfRestartFile.Equals(DateTime.Today.ToShortDateString()) && _restartTrigger == false)
                        {
                            Thread.Sleep(1000);

                            //string restartComputerSetting3 = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, "RestartComputer");

                            string restartComputerSetting3 = SettingsCache.Position["RestartComputer"];

                            //Fetches the time set by the user from the cache
                            //string clock = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, "RestartTime");

                            string clock = SettingsCache.Position["RestartTime"];

                            //Debug.WriteLine(clock);
                            //Debug.WriteLine(DateTime.Now.ToString("t"));
                            //Debug.WriteLine(clock.Length == 1 && DateTime.Now.ToString("t").Equals("0" + clock + ":00"));

                            /* Step 1:
                             * Check clock setting length. That means is the length = 1, then it means that the user has set a time between 2 and 9 o'clock.
                             * If the clock length = 1 and DateTime.Now = 09:00 o'clock and it is the same as 0 '9' :00 => value 9 is from eBRestarterSetting then do the next step                                                                                                                                                                                                                                          
                             */

                            if (clock.Length == 1 && DateTime.Now.ToString("t").Equals("0" + clock + ":00"))
                            {
                                // Step 2: All application will be closed
                                //windows.CloseAllApplications();
                                //windows.CloseProgramsAndShutdown();
                                Thread.Sleep(3000);

                                //Step 3: Then shutdown the entire machine
                                //Process.Start("shutdown.exe", "-r -t 0");
                                Process.Start("shutdown", "/r /f /t 0");

                                //Debug.WriteLine("Hit Restart");
                                Environment.Exit(0);
                            }

                            //Check clock setting length. That means is the length = 1, then it means that the user has set a time between 10 and 23 o'clock. 
                            else if (clock.Length == 2 && DateTime.Now.ToString("t").Equals(clock + ":00"))
                            {
                                //windows.CloseAllApplications();
                                //windows.CloseProgramsAndShutdown();
                                Thread.Sleep(3000);

                                //Process.Start("shutdown.exe", "-r -t 0");

                                Process.Start("shutdown", "/r /f /t 0");
                                Environment.Exit(0);
                            }

                            //Terminate the loop if the user deactivate the restart task
                            else if (restartComputerSetting3.Equals("false"))
                            {
                                _restartTrigger = true;
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
