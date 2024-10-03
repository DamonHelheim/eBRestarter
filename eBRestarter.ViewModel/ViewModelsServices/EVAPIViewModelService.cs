using eBRestarter.Classes.EBRest;
using eBRestarter.Classes.Enums;
using Newtonsoft.Json;
using System.Globalization;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Model.Models;

namespace eBRestarter.ViewModel.ViewModelsServices
{
    public class EVAPIViewModelService : IAPIWebLinks
    {
        private EVisitorAPIJSONDataValues.IPData? EvAPIJDV_IP_DATA = new();
        private EVisitorAPIJSONDataValues.HourlyEarning? EvAPIJDV_Hourly_Earnings = new();
        private EVisitorAPIJSONDataValues.MonthlyEarningsRoot? EvAPIJDV_Monthly_Earnings = new();

        private readonly EBRestSharp eBRestSharp = new();

        private string _username = string.Empty;
        private string _apiKey = string.Empty;

        private EVIPDataValues? eVIPDataValues;

        public string Username { get => _username; set => _username = value; }
        public string Apikey { get => _apiKey; set => _apiKey = value; }


        public EVIPDataValues Get_EV_IP_DataInfo
        {
            get
            {
                eBRestSharp.URL = IAPIWebLinks.IPLINK;
                try
                {
                    ResponseCode responseCode = eBRestSharp.GetRequestStatus();

                    EvAPIJDV_IP_DATA = JsonConvert.DeserializeObject<EVisitorAPIJSONDataValues.IPData?>(eBRestSharp.GetJSONResponse!);

                    eVIPDataValues = new EVIPDataValues
                    {
                        IP = EvAPIJDV_IP_DATA!.IP,
                        Host = EvAPIJDV_IP_DATA.Host,
                        CountryName = EvAPIJDV_IP_DATA.CountryName,
                        CountryCode = EvAPIJDV_IP_DATA.CountryCode
                    };

                    return eVIPDataValues;
                }
                catch (Exception)
                {
                    eVIPDataValues = new EVIPDataValues
                    {
                        IP = "Abfragefehler",
                        Host = "Abfragefehler",
                        CountryName = "Abfragefehler",
                        CountryCode = "Abfragefehler"
                    };

                    return eVIPDataValues;
                }
            }
        }

        public double[]? HourlyEarnings
        {
            get
            {
                if ((_username == null && _apiKey == null) ^ (!(_username == null) && _apiKey == null) ^ (_username == null && !(_apiKey == null)))
                {
                    return null;
                }
                else
                {
                    try
                    {
                        eBRestSharp.URL = IAPIWebLinks.Hourly_Earnings;
                        eBRestSharp.UserName = _username!;
                        eBRestSharp.Password = _apiKey!;

                        ResponseCode responseCode = eBRestSharp.GetRequestStatus();

                        EvAPIJDV_Hourly_Earnings = JsonConvert.DeserializeObject<EVisitorAPIJSONDataValues.HourlyEarning?>(eBRestSharp.GetJSONResponse!);

                        _ = new double[EvAPIJDV_Hourly_Earnings!.EarningsHourly.Length];

                        double[]? getHourlyEarning = EvAPIJDV_Hourly_Earnings.EarningsHourly;

                        for (int i = 0; i < getHourlyEarning.Length; i++)
                        {
                            getHourlyEarning[i] = (int)getHourlyEarning[i]; //convert double in int value
                        }

                        return getHourlyEarning;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        public double[]? HourlyEarningsUnixDynamicAsync
        {
            get
            {
                if ((_username == null && _apiKey == null) ^ (!(_username == null) && _apiKey == null) ^ (_username == null && !(_apiKey == null)))
                {
                    return null;
                }
                else
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        DateTime startDateOfTheMonth = new(now.Year, now.Month, now.Day, 0, 0, 0);

                        long unixTime_StartDateOfTheMonth = ((DateTimeOffset)startDateOfTheMonth).ToUnixTimeSeconds();
                        long unixTimeNow = ((DateTimeOffset)now).ToUnixTimeSeconds();

                        eBRestSharp.URL = IAPIWebLinks.EARNINGS_THIS_MONTH + unixTime_StartDateOfTheMonth + "-" + unixTimeNow;
                        eBRestSharp.UserName = _username!;
                        eBRestSharp.Password = _apiKey!;

                        dynamic? jsonString = JsonConvert.DeserializeObject(eBRestSharp.GetRequestAsync().Result!);

                        double[] getHourlyEarning = new double[24];

                        int iterator = 0;

                        foreach (var item in jsonString!)
                        {
                            getHourlyEarning[iterator] = (int)(double)item.value;
                            iterator++;
                        }

                        return getHourlyEarning;

                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }


        public Tuple<double[]?, int> HourlyEarningsDynamic
        {
            get
            {
                if ((_username == null && _apiKey == null) ^ (!(_username == null) && _apiKey == null) ^ (_username == null && !(_apiKey == null)))
                {
                    Tuple<double[]?, int> getHourlyEarning_and_last_hour = new(null, -1);

                    return getHourlyEarning_and_last_hour;
                }
                else
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        DateTime startDateOfTheMonth = new(now.Year, now.Month, now.Day, 0, 0, 0);

                        long unixTime_StartDateOfTheMonth = ((DateTimeOffset)startDateOfTheMonth).ToUnixTimeSeconds();
                        long unixTimeNow = ((DateTimeOffset)now).ToUnixTimeSeconds();

                        eBRestSharp.URL = IAPIWebLinks.EARNINGS_THIS_MONTH + unixTime_StartDateOfTheMonth + "-" + unixTimeNow;
                        eBRestSharp.UserName = _username!;
                        eBRestSharp.Password = _apiKey!;

                        ResponseCode responseCode = eBRestSharp.GetRequestStatus();

                        dynamic? jsonString = JsonConvert.DeserializeObject(eBRestSharp.GetJSONResponse!);

                        //string a = jsonString!.ToString();

                        //Debug.WriteLine(a + " JSON");

                        double[] getHourlyEarning = new double[24];

                        //Array mit 0.0 Werten initialisieren
                        Array.Fill(getHourlyEarning, 0.0);

                        int stop_24_iterator = 0;
                        int valueToSet_hour = 0;

                        foreach (var item in jsonString!)
                        {
                            string dateTimeHourlyToday = item.from_w3c;

                            DateTime dateTimeHourToday = DateTime.ParseExact(dateTimeHourlyToday, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                            DateTime today = DateTime.Now;

                            bool isDateFromYesterday = dateTimeHourToday.Date != today.Date;

                            if (isDateFromYesterday is true)
                            {
                                continue;

                            }
                            else
                            {

                                valueToSet_hour = dateTimeHourToday.Hour;

                                if (stop_24_iterator is 24) { break; }

                                getHourlyEarning[valueToSet_hour] = (int)(double)item.value;

                                stop_24_iterator++;
                            }
                        }

                        Tuple<double[]?, int> getHourlyEarning_and_last_hour = new(getHourlyEarning, valueToSet_hour);

                        return getHourlyEarning_and_last_hour;

                    }
                    catch (Exception)
                    {
                        Tuple<double[]?, int> getHourlyEarning_and_last_hour = new(null, -1);

                        return getHourlyEarning_and_last_hour;
                    }
                }
            }
        }


        public string EarningsSumThisMonth
        {
            get
            {
                if ((_username == null && _apiKey == null) ^ (!(_username == null) && _apiKey == null) ^ (_username == null && !(_apiKey == null)))
                {
                    return 0.ToString();
                }
                else
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        DateTime startDateOfTheMonth = new(now.Year, now.Month, 1); //01.07.2024 00:00:00               

                        long unixTime_StartDateOfTheMonth = ((DateTimeOffset)startDateOfTheMonth).ToUnixTimeSeconds();
                        long unixTimeNow = ((DateTimeOffset)now).ToUnixTimeSeconds();

                        eBRestSharp.URL = IAPIWebLinks.EARNINGS_THIS_MONTH + unixTime_StartDateOfTheMonth + "-" + unixTimeNow;
                        eBRestSharp.UserName = _username!;
                        eBRestSharp.Password = _apiKey!;

                        ResponseCode responseCode = eBRestSharp.GetRequestStatus();

                        EvAPIJDV_Monthly_Earnings = JsonConvert.DeserializeObject<EVisitorAPIJSONDataValues.MonthlyEarningsRoot>("{" + "\"Points_Total_This_Month\"" + ":" + eBRestSharp.GetJSONResponse + "}"); //Used to format the JSON query so that it conforms to the array so that it can be called object-related.

                        int MonthlySum = 0;

                        foreach (var item in EvAPIJDV_Monthly_Earnings!.Points_Total_This_Month!)
                        {
                            MonthlySum = (int)double.Parse(item.Value!.Replace('.', ',')) + MonthlySum;
                        }

                        return MonthlySum.ToString();
                    }
                    catch (Exception)
                    {
                        return "Abfragefehler";
                    }
                }
            }
        }

        public EVAPIViewModelService()
        {

        }

        public EVAPIViewModelService(string username, string apikey)
        {
            Username = username;
            Apikey = apikey;
        }

        public string AuthenticateInfo(string username, string password)
        {
            eBRestSharp.URL = IAPIWebLinks.Hourly_Earnings;
            eBRestSharp.UserName = username;
            eBRestSharp.Password = password;

            ResponseCode checkAuthentication = eBRestSharp.GetRequestStatus();

            if (checkAuthentication is ResponseCode.Success)
            {
                return "200";

            }
            else if (checkAuthentication is ResponseCode.RequestLimit)
            {
                return "Dein Anfragelimit ist gleich überschritten.";
            }
            else if (checkAuthentication is ResponseCode.NoConnectionToServer)
            {
                return "Es konnte keine Verbindung zum Abfrageserver gestellt werden.";
            }
            else if (checkAuthentication is ResponseCode.HttpRE429)
            {
                return "Das API Abfragelimit wurde überschritten, bitte versuche es später erneut.";
            }
            else if (checkAuthentication is ResponseCode.HttpRE401)
            {
                return "Benutzername oder der API Schlüssel sind ungültig.";
            }
            else if (checkAuthentication is ResponseCode.InternalServerError)
            {
                return "Internal Server Error: Eventuell sind Nutzername und API Schlüssel ungültig.";
            }
            else if (checkAuthentication is ResponseCode.HTTPTimeout)
            {
                return "Die Abfrage auf den Server dauert zu lange, bitte versuche es später erneut.";
            }
            else
            {
                return "Es ist ein Fehler aufgetreten.";
            }
        }
    }
}
