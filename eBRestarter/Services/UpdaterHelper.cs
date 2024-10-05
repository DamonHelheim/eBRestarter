using AutoUpdaterDotNET;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace eBRestarter.Services
{
    public class UpdaterHelper
    {

        private const string UpdateHost = "https://raw.githubusercontent.com/DamonHelheim/eBRestarter/refs/heads/main/eBRestarter/Resources/XML/eBRestarterUpdate.xml";

        public UpdaterHelper() {}

        public static bool NewVersionAvailable()
        {
            try
            {
                XDocument xdoc = XDocument.Load(UpdateHost);

                string newVersion = xdoc.Descendants("version").First().Value.Replace(".", ""); 
                
                string currentVersion = VersionHelper.GetCurrentVersion!.Replace(".", "");

                if (Int32.Parse(newVersion) > Int32.Parse(currentVersion))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                //Log.Error(e, "Es ist eine Ausnahme im UpdateHelperbereich aufgetreten. Logging...");
                return false;
            }
        }


        private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error is null)
            {
                if (args.IsUpdateAvailable)
                {
                    DialogResult dialogResult;

                    if (args.Mandatory.Value)
                    {

                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                                DecrementCheckForUpdateEvent();
                    }
                    else
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"Es ist eine neue Version ({args.CurrentVersion}) verfügbar. Aktuell ist die Version {args.InstalledVersion} installiert. Möchtest Du jetzt für diese Anwendung ein Update durchführen?", @"Update verfügbar",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                                DecrementCheckForUpdateEvent();
                    }

                    // Uncomment the following line if you want to show standard update dialog instead.
                    // AutoUpdater.ShowUpdateForm(args);

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                DecrementCheckForUpdateEvent();
                                Application.Exit();
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            DecrementCheckForUpdateEvent();
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"Zurzeit gibt es keine Updates, bitte versuche es später erneut.", @"Keine Updates verfügbar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DecrementCheckForUpdateEvent();
                }
            }
            else
            {
                if (args.Error is WebException)
                {
                    MessageBox.Show(
                        @"Der Updateserver ist nicht erreichbar. Bitte prüfe deine Internetverbindung oder versuche es später erneut.",
                        @"Updateprüfung fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DecrementCheckForUpdateEvent();
                }
                else
                {
                    MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                        DecrementCheckForUpdateEvent();
                }
            }
        }

        public static void ManualUpdate()
        {
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Start(UpdateHost);
        }

        public static void DecrementCheckForUpdateEvent()
        {
            AutoUpdater.CheckForUpdateEvent -= AutoUpdaterOnCheckForUpdateEvent;
        }

        public void AutoUpdate()
        {
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Start(UpdateHost);
        }

        public void StartDownload()
        {
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            if (currentDirectory.Parent != null)
            {
                AutoUpdater.InstallationPath = currentDirectory.Parent.FullName;
            }
        }
    }
}

