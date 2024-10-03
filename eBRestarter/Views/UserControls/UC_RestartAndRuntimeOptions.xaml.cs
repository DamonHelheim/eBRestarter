using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_RestartAndRuntimeOptions : UserControl
    {
        public RestartAndRuntimeOptionsViewModel RestartAndRuntimeOptionsViewModel { get; } = new();

        private readonly bool valueChanged = false;

        public UC_RestartAndRuntimeOptions()
        {
            InitializeComponent();
            
            DataContext = RestartAndRuntimeOptionsViewModel;

            valueChanged = true;
        }


        //Allows only numbers on Textbox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");

            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }

            //e.Handled = regex.IsMatch(e.Text);

            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
        }

        private void TxtB_Browser_Waiting_Time_To_Start_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (int.Parse(TxtB_Browser_Waiting_Time_To_Start.Text) > 60)
                {
                    SliderBrowserWaitingTimeToStart.Value = 60;
                    TxtB_Browser_Waiting_Time_To_Start.Text = 60.ToString();
                    BrowserWaitingTimeToStart();

                }
                else if (int.Parse(TxtB_Browser_Waiting_Time_To_Start.Text) < 20)
                {
                    SliderBrowserWaitingTimeToStart.Value = 20;
                    TxtB_Browser_Waiting_Time_To_Start.Text = 20.ToString();
                    BrowserWaitingTimeToStart();
                }
                else
                {
                    BrowserWaitingTimeToStart();
                }
            }
        }

        private void BrowserWaitingTimeToStart()
        {
            SliderBrowserWaitingTimeToStart.Value = int.Parse(TxtB_Browser_Waiting_Time_To_Start.Text);
            string a = ((int)SliderBrowserWaitingTimeToStart.Value).ToString();
            RestartAndRuntimeOptionsViewModel.Browser_waiting_time(a);
        }


        private void Txt_Browser_Runtime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (int.Parse(Txt_Browser_Runtime.Text) > 12)
                {
                    SliderBrowserRuntime.Value = 12;
                    Txt_Browser_Runtime.Text = 12.ToString();
                    Browser_Runtime();
                }
                else if (int.Parse(Txt_Browser_Runtime.Text) < 1)
                {
                    SliderBrowserRuntime.Value = 1;
                    Txt_Browser_Runtime.Text = 1.ToString();
                    Browser_Runtime();
                }
                else
                {
                    Browser_Runtime();
                }
            }
        }

        private void Browser_Runtime()
        {
            SliderBrowserRuntime.Value = int.Parse(Txt_Browser_Runtime.Text);
            string a = ((int)SliderBrowserWaitingTimeToStart.Value).ToString();
            RestartAndRuntimeOptionsViewModel.Browser_runtime(a);
        }

        private void TxtB_Browser_Waiting_Time_To_Start_LostFocus(object sender, RoutedEventArgs e)
        {
            if (valueChanged == true)
            {
                SliderBrowserWaitingTimeToStart.Value = int.Parse(TxtB_Browser_Waiting_Time_To_Start.Text);
                string a = ((int)SliderBrowserWaitingTimeToStart.Value).ToString();
                RestartAndRuntimeOptionsViewModel.Browser_waiting_time(a);
            }
        }



        private void TxtB_Browser_Runtime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (valueChanged == true)
            {
                SliderBrowserRuntime.Value = int.Parse(Txt_Browser_Runtime.Text);

                string a = ((int)SliderBrowserWaitingTimeToStart.Value).ToString();

                RestartAndRuntimeOptionsViewModel.Browser_runtime(a);
            }
        }



        private void SliderBrowserWaitingTimeToStart_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //valueChanged is set to false when the application is started and only becomes true when the GUI ini was successful.
            //This prevents the slider valueChanged method, which is initialized before GUI INitialize, from not saving a
            //Additional or the initial value 20.
            if (valueChanged == true)
            {
                string a = ((int)SliderBrowserWaitingTimeToStart.Value).ToString();

                RestartAndRuntimeOptionsViewModel.Browser_waiting_time(a);
            }
        }

        private void SliderBrowserRuntime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (valueChanged == true)
            {
                string a = ((int)SliderBrowserRuntime.Value).ToString();

                RestartAndRuntimeOptionsViewModel.Browser_runtime(a);
            }
        }

    }
}
