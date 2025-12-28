using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls
{
    public sealed partial class UC_RestartAndRuntimeOptions : UserControl
    {
        public UC_RestartAndRuntimeOptions()
        {
            InitializeComponent();
        }

        //private void TxtB_Browser_Waiting_Time_To_Start_KeyDown(object sender, KeyRoutedEventArgs e)
        //{
        //    // In WinUI 3 heißt es 'VirtualKey.Enter'
        //    if (e.Key == VirtualKey.Enter)
        //    {
        //        // Verhindert den "Ping"-Sound von Windows
        //        e.Handled = true;

        //        // 1. Sicherstellen, dass es eine Zahl ist (verhindert Absturz bei leerem Feld)
        //        if (int.TryParse(TxtB_Browser_Waiting_Time_To_Start.Text, out int enteredValue))
        //        {
        //            // 2. Wert begrenzen (Clamping)
        //            // Wenn < 20 -> wird 20. Wenn > 60 -> wird 60.
        //            int validatedValue = Math.Clamp(enteredValue, 20, 60);

        //            // 3. UI aktualisieren (nur wenn sich was geändert hat, um Flackern zu vermeiden)
        //            if (SliderBrowserWaitingTimeToStart.Value != validatedValue)
        //            {
        //                SliderBrowserWaitingTimeToStart.Value = validatedValue;
        //            }

        //            if (enteredValue != validatedValue)
        //            {
        //                TxtB_Browser_Waiting_Time_To_Start.Text = validatedValue.ToString();
        //                // Cursor ans Ende setzen, sonst springt er nach vorne
        //                TxtB_Browser_Waiting_Time_To_Start.SelectionStart = TxtB_Browser_Waiting_Time_To_Start.Text.Length;
        //            }

        //            // 5. Optional: Fokus von der TextBox nehmen (damit der User sieht, dass es fertig ist)
        //            // TxtB_Browser_Waiting_Time_To_Start.IsEnabled = false; 
        //            // TxtB_Browser_Waiting_Time_To_Start.IsEnabled = true; 
        //        }
        //        else
        //        {
        //            // Fallback, wenn keine Zahl eingegeben wurde (z.B. auf Slider-Wert zurücksetzen)
        //            TxtB_Browser_Waiting_Time_To_Start.Text = SliderBrowserWaitingTimeToStart.Value.ToString();
        //        }
        //    }
        //}
    }
}
