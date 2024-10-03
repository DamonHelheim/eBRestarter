namespace eBRestarter.Classes.EBScheduler
{
    public sealed class CountdownVisuals
    {

        private delegate string? ShowCurrentCountdownTimerHourMinutesSeconds(int hoursLeft, int minutesLeft, int seconds_left);
        private delegate string? ShowCurrentCountdownTimerMinutesSeconds(int minutesLeft, int seconds_left);
        private delegate string? ShowCurrentCountdownTimerSeconds(int seconds_left);

        private readonly ShowCurrentCountdownTimerHourMinutesSeconds ShowHourMinutesSecondsCount = (hoursLeft, minutesLeft, seconds_left) => hoursLeft + "h: " + minutesLeft + "m: " + seconds_left + "s";
        private readonly ShowCurrentCountdownTimerMinutesSeconds ShowMinutesSecondsCount = (minutesLeft, seconds_left) => minutesLeft + "m: " + seconds_left + "s";
        private readonly ShowCurrentCountdownTimerSeconds ShowSecondsCount = (seconds_left) => seconds_left + "s";

        private delegate int CalculateTime(int seconds, int mod);

        private readonly CalculateTime CalculateTimeSequences = (seconds, mod) => seconds % mod;

        //Class variable
        private int _seconds;

        //Properties
        public int SetSeconds { set => _seconds = value; }
        public int GetSeconds { get => _seconds; }


        public string ShowCurrentTime()
        {
            if (GetSeconds >= 0 && GetSeconds <= 60)
            {
                return GetTimeSeconds();

            }
            else if (GetSeconds >= 61 && GetSeconds <= 3600)
            {
                return GetTimeMinutesAndSeconds();

            }
            else
            {
                return GetTimeHourAndMinutesAndSeconds();
            }
        }


        //This method construate the timer format in hours, minutes and seconds e. g. 5h:10m:50s 
        public string GetTimeHourAndMinutesAndSeconds()
        {

            int seconds_left = CalculateTimeSequences(GetSeconds, 60);
            /* 
            * e.g. 3599 seconds mod 60 = 59 seconds or 3569 mod 60 = 29 seconds it shows only seconds
            * 
            * Example 1.) 180 seconds % 60 = 0 seconds => 60 * 3 + 0 = 180 seconds
            * 
            * Example 2.) 179 seconds % 60 = 59 seconds => 60 * 2 + 59 = 179 seconds 
            */

            int minutes = (GetSeconds - seconds_left) / 60;     /*
                                                                 * Example 1.) ( 'GetSeconds' 180 -  'seconds_left' 0) / 60 = 3
                                                                 * 
                                                                 * Example 2.) ( 'GetSeconds' 179 -  'seconds_left' 59 = 120 seconds) / 60 = 2
                                                                 * 
                                                                 *             ( 'GetSeconds' 178 -  'seconds_left' 58 = 120 seconds) / 60 = 2
                                                                 */


            int minutesLeft = CalculateTimeSequences(minutes, 60);
            /*
            * Example 1.) 3 % 60 = 3 minutes
            * 
            * Example 2.) 2 % 60 = 2 minutes
            * 
            */

            int hours = GetSeconds / 60 / 60;                /*
                                                                  * The integer rounds off the number, e.g. 2.99 to 2.
                                                                  * 
                                                                  * Example 1.) ((GetSeconds 10800 / 60) / 60) = 3
                                                                  * 
                                                                  * Example 2.) ((GetSeconds 10799 / 60) / 60) = 2
                                                                  * 
                                                                  */


            int hoursLeft = CalculateTimeSequences(hours, 60);    /*
                                                                   * 3 % 60 = 3 hours
                                                                   * 2 % 60 = 2 hours
                                                                   */


            --_seconds;                                            //Every new method call decrements the seconds

            return ShowHourMinutesSecondsCount(hoursLeft, minutesLeft, seconds_left)!; //*hoursLeft + "h: " + minutesLeft + "m: " + seconds_left + "s";*/
        }

        //This method construate the timer format in minutes and seconds e.g. 10m:50s 
        public string GetTimeMinutesAndSeconds()
        {
            int seconds_left = CalculateTimeSequences(GetSeconds, 60);

            int minutes = (GetSeconds - seconds_left) / 60;
            int minutesLeft = CalculateTimeSequences(minutes, 60);

            --_seconds;

            return ShowMinutesSecondsCount(minutesLeft, seconds_left)!; //minutesLeft + "m: " + seconds_left + "s";
        }

        //This method construate the timer format in  seconds e.g. 50s 
        public string GetTimeSeconds()
        {
            int seconds_left = CalculateTimeSequences(GetSeconds, 60);

            --_seconds;

            return ShowSecondsCount(seconds_left)!; //seconds_left + "s";
        }
    }
}
