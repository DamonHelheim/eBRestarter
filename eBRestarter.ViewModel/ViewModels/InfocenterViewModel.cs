using eBRestarter.Classes.OperatingSystem;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Model.Models;
using System.Diagnostics;
using System.Management;
using Serilog;
using eBRestarter.Classes.Converter;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class InfocenterViewModel : BaseViewModel
    {

        #pragma warning disable CA1416 // Validate platform compatibility
        private readonly ManagementObjectSearcher myProcessorObject        = new("select * from Win32_Processor");
        private readonly ManagementObjectSearcher myVideoObject            = new("select * from Win32_VideoController");
        private readonly ManagementObjectSearcher RAM                      = new("SELECT * FROM Win32_OperatingSystem");

        private readonly ManagementObjectSearcher  myOperativeSystemObject  = new("select * from Win32_OperatingSystem");

        private readonly ConvertSizeSuffixes       css                      = new();

        private HardwareSpecification  HardwareSpecification { get; set; } = new();
        public WindowsInformation       WindowsInformations { get; set; }   = new();
        public WindowsOS                WindowsOS { get; } = new();
        public HardwareSpecification HardwareSpecificationModel { get => HardwareSpecification; set { HardwareSpecification = value; } }
        public WindowsInformation WindowsInformationsModel { get => WindowsInformations; set { WindowsInformations = value; } }


        public InfocenterViewModel() {

            Titel = "Infocenter";

            LoadHardwareAndSoftwareSpecification();

            //GetCurrentOSBuildInformation(WindowsSpecification.GetCurrentOSVersion());

        }

        [RelayCommand]
#pragma warning disable CA1822 // Mark members as static
        public void OpenSupportWebsite(object url)
#pragma warning restore CA1822 // Mark members as static
        {
            Process.Start(new ProcessStartInfo(url as string) { UseShellExecute = true });
        }

        private void LoadHardwareAndSoftwareSpecification()
        {
            Task.Run(LoadHardwareAndSoftwareSpecificationAsync);
            //Task.Run(async () => await LoadHardwareSpecificationAsync());
        }

        private readonly string errorMessageOSInfo = "Es ist eine Ausnahme beim Auslesen der Betriebssysteminformationen aufgetreten. Logging...";

        private async Task LoadHardwareAndSoftwareSpecificationAsync()
        {          
            await Task.Run(() => {

                try
                {
                    foreach (ManagementObject? obj in myProcessorObject!.Get().Cast<ManagementObject?>())
                    {
                        HardwareSpecificationModel!.Processor = "Prozessor: " + obj!["Name"].ToString();
                    }

                } catch(Exception e)
                {
                    Log.Error(e, errorMessageOSInfo);
                }
                
            });
                      
            await Task.Run(() => {

                try
                {

                    foreach (ManagementObject obj in myVideoObject!.Get().Cast<ManagementObject>())
                    {
                        HardwareSpecificationModel!.Graphics = "Grafikkarte: " + obj!["Name"].ToString();
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e, errorMessageOSInfo);
                }

            });


            await Task.Run(() => {

                try
                {
                    foreach (ManagementObject result in RAM!.Get())
                    {
                        HardwareSpecificationModel!.Ram = "Installierter RAM: " + css.SizeSuffix((long)Convert.ToDouble(result["TotalVisibleMemorySize"]) * 1024);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, errorMessageOSInfo);
                }

            });

            await Task.Run( () => {

                try
                {
                    foreach (ManagementObject obj in myOperativeSystemObject.Get().Cast<ManagementObject>())
                    {
                        WindowsInformationsModel.CurrentEdition = "Edition: " + obj["Caption"].ToString();
                    }

                    WindowsInformationsModel.CurrentOSBuildVersion = "Betriebssystembuild: " + WindowsOS.GetCurrentOSBuildVersion();

                    WindowsInformationsModel.CurrentOSVersion = "Version: " + WindowsOS.GetCurrentOSVersion();

                }

                catch (Exception e)
                {
                    Log.Error(e, errorMessageOSInfo);
                }

            });
        }
    }
}
