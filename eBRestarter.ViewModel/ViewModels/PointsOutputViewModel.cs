using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using Serilog;
using eBRestarter.ViewModel.ViewModelsServices;
using eBRestarter.Classes.Cache;
using eBRestarter.Model.Models;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class PointsOutputViewModel : BaseViewModel
    {

        private bool ResetChartTrigger = false;

        private readonly EVAPIViewModelService? ev2 = new();

        private ObservableCollection<ObservableValue>? _observableValues;
        public ObservableCollection<ISeries>? Series { get; set; }

        [ObservableProperty]
        private string chartTitle = string.Empty;

        [ObservableProperty]
        public string earningsThisMonthSum = string.Empty;

        [ObservableProperty]
        public string earningsThisDaySum = string.Empty;

        [ObservableProperty]
        public EVIPDataValues? iP_DataInfo;

        [ObservableProperty]
        public string ipAdress = string.Empty;

        [ObservableProperty]
        public string host = string.Empty;

        [ObservableProperty]
        public string countryName = string.Empty;

        [ObservableProperty]
        public string countryCode = string.Empty;

        [ObservableProperty]
        public string clockNextEarningsRefresh = string.Empty;

        [ObservableProperty]
        public string currentMonth = string.Empty;

        [ObservableProperty]
        public string currentDay = string.Empty;

        [ObservableProperty]
        public string eVisitorAPIActivatedInfo = string.Empty;

        private short time = 0;

        public PointsOutputViewModel() 
        {
            double[]? hourlyEarnings = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]; 

            InitializeBarChartValues(hourlyEarnings);

            UpdateValuesTask();
        }


        private void APIUpdateValues(string APIUsername, string APIKey, bool InitializeFlag)
        {
            try
            {
                string? checkAuthentication = ev2!.AuthenticateInfo(APIUsername, APIKey);

                if (checkAuthentication.Contains("200") && InitializeFlag is true)
                {
                    ev2.Username = SettingsCache.APIPosition["APIUsername"];
                    ev2.Apikey = SettingsCache.APIPosition["APIKey"];

                    //Fill item
                    UpdateEBAPIValues(true);

                }
                else if (checkAuthentication.Contains("200") && InitializeFlag is false)
                {
                    ev2.Username = SettingsCache.APIPosition["APIUsername"];
                    ev2.Apikey = SettingsCache.APIPosition["APIKey"];

                    //update item
                    UpdateEBAPIValues(false);

                    //EarningsThisDaySum = "BTP: " + values.Sum();
                }

            } catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme in bei" + nameof(APIUpdateValues) + "aufgetreten. Logging...");
            }         
        }


        public void UpdateEBAPIValues(bool UpdateOrFill)
        {
            Tuple<double[]?, int> getHourlyEarning_and_last_hour = ev2!.HourlyEarningsDynamic;

            if (getHourlyEarning_and_last_hour.Item1 is null)
            {
                EarningsThisDaySum  = "BTP: " + 0;
            }
            else
            {
                EarningsThisDaySum  = "BTP: " + getHourlyEarning_and_last_hour.Item1.Sum();
            }

            EarningsThisMonthSum    = "BTP: "           + ev2.EarningsSumThisMonth;

            IP_DataInfo             =                     ev2.Get_EV_IP_DataInfo;

            IpAdress                = "IP: " + IP_DataInfo.IP;
            Host                    = "Host: " + IP_DataInfo.Host;
            CountryName             = "Country code: " + IP_DataInfo.CountryName;
            CountryCode             = "Country name: " + IP_DataInfo.CountryCode;

            if (UpdateOrFill is true)
            {
                FillItem(getHourlyEarning_and_last_hour.Item1!);

            } else
            {
                UpdateItem(getHourlyEarning_and_last_hour.Item1!, getHourlyEarning_and_last_hour.Item2!);
            }
        }


        public void UpdateItem(double[] labelValues, int hourIndexPosition)
        {
            try
            {
                if (labelValues is null)
                {
                    return;

                }
                else
                {
                    double[] LabelValuesUpdate = labelValues;

                    int addedValue = (int)LabelValuesUpdate[hourIndexPosition];

                    // we grab the last instance in our collection
                    ObservableValue lastInstance = _observableValues![hourIndexPosition];

                    // finally modify the value property and the chart is updated!
                    lastInstance.Value = addedValue;


                    /*
                     * Old code unused
                     * 
                     * int countUntilZero = 0;

                        foreach (int i in LabelValuesUpdate.Select(v => (int)v))
                        {
                            if (i is 0)
                            {
                                break;
                            }
                            else
                            {
                                countUntilZero++;
                            }
                        }

                        int addedValue = (int)LabelValuesUpdate[countUntilZero - 1];

                        // we grab the last instance in our collection
                        ObservableValue lastInstance = _observableValues![countUntilZero - 1];

                        // finally modify the value property and the chart is updated!
                        lastInstance.Value = addedValue;
                        }
                     */
                }

                } catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme in bei " + nameof(UpdateItem) + " aufgetreten. Logging...");
            }         
        }


        public void FillItem(double[] labelValues)
        {
            try
            {
                if(labelValues is null)
                {
                    return;

                } else
                {
                    for (int i = 0; i < labelValues!.Length; i++)
                    {
                        //We grab the last instance in our collection
                        ObservableValue lastInstance = _observableValues![i];

                        //Finally modify the value property and the chart is updated!
                        lastInstance.Value = labelValues[i];
                    }
                }

            } catch(Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme in bei " + nameof(FillItem) + " aufgetreten. Logging...");
            }
            
        }


        public void InitializeBarChartValues(double[] labelValues)
        {
            try
            {
                if (labelValues is null)
                {
                    return;
                }
                else
                {
                    _observableValues = new ObservableCollection<ObservableValue> { };

                    foreach (var value in labelValues)
                    {
                        _observableValues.Add(new ObservableValue(value));
                    }

                    Series = new ObservableCollection<ISeries> {

                         new ColumnSeries<ObservableValue> {

                                Values = _observableValues,
                                Name = "Punkte",
                                DataLabelsPaint = new SolidColorPaint(new SKColor(12, 142, 168)), //Colors of the data labels
                                DataLabelsPosition = DataLabelsPosition.Top,
                                Stroke = null,
                                DataLabelsSize=14,
                                Mapping = null,
                                
                                //Fill = new SolidColorPaint(SKColors.SteelBlue),
                                //Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),
                                //Fill = null
                         }
                    };

                }
            } catch(Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme in bei " + nameof(InitializeBarChartValues) + " aufgetreten. Logging...");
            }

        }


        public Axis[] XAxes { get; set; } = [

            new Axis
            {
                Name = "Uhrzeit",
                NamePaint = new SolidColorPaint
                {
                    Color = new SKColor(109, 142, 168),
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                    //FontFamily = "Times New Roman",
                },
                NameTextSize = 14,
                TextSize = 14,
                MinStep = 1,
                ForceStepToMin = true,
                LabelsPaint = new SolidColorPaint
                {
                    Color = new SKColor(109, 142, 168),
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                    //FontFamily = "Times New Roman",
                },
               
                //MinZoomDelta = 1,          
                //LabelsRotation = 15,
            }
        ];


        public Axis[] YAxes { get; set; } =
        [
            new Axis
            {
                Name = "Punkteanzahl",
                NamePaint = new SolidColorPaint
                {
                    Color = new SKColor(109, 142, 168),
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                    //FontFamily = "Times New Roman",
                },
                NameTextSize = 14,
                TextSize = 14,
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                MinStep = 1,
                LabelsPaint = new SolidColorPaint
                {
                    Color = new SKColor(109, 142, 168),
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                    //FontFamily = "Times New Roman",
                },

                SeparatorsPaint = new SolidColorPaint
                {
                    Color = new SKColor(109, 142, 168),
                    SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
                    //FontFamily = "Times New Roman",
                },
            
                // Use the Labeler property to give format to the axis values 
                // Now the Y axis we will display it as currency
                // LiveCharts provides some common formatters
                // in this case we are using the currency formatter.
                Labeler = Labelers.Default

                // you could also build your own currency formatter
                // for example:
                // Labeler = (value) => value.ToString("C")

                // But the one that LiveCharts provides creates shorter labels when
                // the amount is in millions or trillions
            }
        ];

        private void UpdateValuesTask()
        {
            Task.Run(UpdateValuesAsync);
        }

        private async Task UpdateValuesAsync()
        {
            await Task.Run(async () => {

                while (true) {

                    if (SettingsCache.APIPosition["APIUsername"] is not null && SettingsCache.APIPosition["APIKey"] is not null)
                    {
                        APIUpdateValues(SettingsCache.APIPosition["APIUsername"], SettingsCache.APIPosition["APIKey"], true);

                        DateTime localDate3         = DateTime.Now;
                        time                        = short.Parse(localDate3.ToString("mm"));
                        DateTime x30MinsLater2      = localDate3.AddMinutes((60 - time) + 5);
                        ClockNextEarningsRefresh    = "Nächste Aktualisierung um " + x30MinsLater2.ToString("HH:mm") + " Uhr";
                        CurrentDay                  = "Punktesumme heute";
                        CurrentMonth                = "Punktesumme " + DateTime.Now.ToString("MMMM");
                        ChartTitle                  = "Surfbar & Klicks Vergütung in BTP: Tagesübersicht " + DateTime.Today.ToString("d");

                        EVisitorAPIActivatedInfo    = "eBesucher Schnittstelle deaktivieren";

                        ResetChartTrigger           = true;
                    }
                    else
                    {
                        EVisitorAPIActivatedInfo    = "eBesucher Schnittstelle aktivieren";
                        ChartTitle                  = "eBesucher API ist nicht aktiviert";
                        CurrentDay                  = "-";
                        CurrentMonth                = "-";
                        EarningsThisDaySum          = "-";
                        EarningsThisMonthSum        = "-";
                        ClockNextEarningsRefresh    = "-";

                        IpAdress                    = "IP: -";
                        Host                        = "Host: -";
                        CountryName                 = "Country code: -";
                        CountryCode                 = "Country name: -";

                        if (ResetChartTrigger is true)
                        {
                            ResetChart();

                            ResetChartTrigger = false;
                        }
                    }

                    while (SettingsCache.APIPosition["APIUsername"] is not null && SettingsCache.APIPosition["APIKey"] is not null) {

                            DateTime localDate  = DateTime.Now;
                            time                = short.Parse(localDate.ToString("mm"));

                            if (time == 5) {

                            if (DateTime.Now.ToString("t").Equals("00:05") is true)
                            {

                                ResetChart();
                                APIUpdateValues(SettingsCache.APIPosition["APIUsername"], SettingsCache.APIPosition["APIKey"], true);
                                DateTime x30MinsLater = localDate.AddMinutes(60);
                                ClockNextEarningsRefresh = "Nächste Aktualisierung um " + x30MinsLater.ToString("HH:mm") + " Uhr";
                                CurrentDay = "Punktesumme heute";
                                CurrentMonth = "Punktesumme " + DateTime.Now.ToString("MMMM");
                                ChartTitle = "Surfbar & Klicks Vergütung in BTP: Tagesübersicht " + DateTime.Today.ToString("d");

                                while (time is 5)
                                {
                                    DateTime localDate2 = DateTime.Now;
                                    time = short.Parse(localDate2.ToString("mm"));

                                    await Task.Delay(1000);
                                }

                            } else
                            {
                                APIUpdateValues(SettingsCache.APIPosition["APIUsername"], SettingsCache.APIPosition["APIKey"], false);
                                DateTime x30MinsLater = localDate.AddMinutes(60);
                                ClockNextEarningsRefresh = "Nächste Aktualisierung um " + x30MinsLater.ToString("HH:mm") + " Uhr";
                                CurrentDay = "Punktesumme heute";
                                CurrentMonth = "Punktesumme " + DateTime.Now.ToString("MMMM");
                                ChartTitle = "Surfbar & Klicks Vergütung in BTP: Tagesübersicht " + DateTime.Today.ToString("d");

                                while (time is 5)
                                {
                                    DateTime localDate2 = DateTime.Now;
                                    time = short.Parse(localDate2.ToString("mm"));

                                    await Task.Delay(1000);
                                }
                            }
                        }
                            await Task.Delay(1000);                          
                    }
                        await Task.Delay(1000);
                }              
            });
        }

        private void ResetChart()
        {
            double[]? hourlyEarnings = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            FillItem(hourlyEarnings);
        }
    }
}
