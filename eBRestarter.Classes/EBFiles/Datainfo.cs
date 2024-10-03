using Serilog;
using System.Text.RegularExpressions;

namespace eBRestarter.Classes.EBFiles
{
    public sealed class Datainfo
    {

        //filePath      @"C:\Users\TestUser\AppData\Roaming\Mozilla\Firefox\profiles.ini";
        //matchString   ".*Default=Profiles/.*";
        //replaceString "Default=Profiles/";

        //Used to find e. g. the folder profile name in Firefox in roaming Data
        public static string GetInfoFromFile(string filePath, string matchString, string replaceString)
        {
            string line;
            string matchedString = "";

            // Read the file and display it line by line.
            try
            {

                /*
                 * This is a excerpt from Firefox profiles.ini
                 * 
                 * [Install308046B0AF4A39CB]
                 * Default=Profiles/dc1vz7y6.default-release
                 * Locked=1

                 * [Profile1]
                 * Name=default
                 * IsRelative=1
                 * Path=Profiles/w4j9vrzn.default
                 * Default=1

                 * [Profile0]
                 * Name=default-release
                 * IsRelative=1
                 * Path=Profiles/dc1vz7y6.default-release

                 * [General]
                 * StartWithLastProfile=1
                 * Version=2
                 * 
                 */

                using StreamReader file = new(filePath);
                //@"C:\Users\TestUser\AppData\Roaming\Mozilla\Firefox\profiles.ini"
                while ((line = file.ReadLine()!) != null)
                {
                    if (Regex.IsMatch(line, matchString))                               // Now search in the file the line with  ".*Default=Profiles/.*"
                    {
                        matchedString = Regex.Replace(line, replaceString, "");         // Now replace "Default=Profiles/" and get only the profile Name of the folder dc1vz7y6.default-release
                    }
                }

                file.Close();

                AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));

                return matchedString;
            }
            catch (FileNotFoundException e)
            {
                Log.Error(e, "Es ist eine Ausnahme im Datainfobereich aufgetreten. Logging...");
                return "";
            }
            catch (DirectoryNotFoundException e)
            {
                Log.Error(e, "Es ist eine Ausnahme im Datainfobereich aufgetreten. Logging...");
                return "";
            }
        }
    }
}
