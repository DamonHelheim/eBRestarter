using eBRestarter.Classes.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using System.ComponentModel.DataAnnotations;
using eBRestarter.ViewModel.ViewModelsServices;
using eBRestarter.Classes.Cache;


namespace eBRestarter.ViewModel.ViewModels
{
    public partial class ActivateAPIAccessViewModel : ObservableValidator
    {

        private readonly EVAPIViewModelService? ev2 = new();

        private readonly IEBFile eBFileManagement = new EBFileManagement();
        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public IAsyncRelayCommand SubmitCommand { get; }

        private string _username = string.Empty;
        private string _apikey = string.Empty;

        [ObservableProperty]
        private string filePathImport = string.Empty;

        [ObservableProperty]
        private string visibillityProgressBar = string.Empty;

        [ObservableProperty]
        private string visibillityText = string.Empty;

        [ObservableProperty]
        private string foregroundColorAccessSuccess = string.Empty;

        [ObservableProperty]
        private string registrationSuccessfulInfo = string.Empty;

        [Required(ErrorMessage = "Benutzername ist erforderlich.")]
        //[MinLength(5, ErrorMessage = "Benutzername muss mindestens 5 Zeichen lang sein.")]
        //[MaxLength(100, ErrorMessage = "Benutzername darf maximal 100 Zeichen haben.")]
        public string Username
        {
            get => _username;
            set
            {            

                SetProperty(ref _username, value);
                ValidateProperty(value!);
               
                SubmitCommand.NotifyCanExecuteChanged();
            }
        }

        [Required(ErrorMessage = "API-Schlüssel ist erforderlich.")]
        public string Apikey
        {
            get => _apikey;
            set
            {
                SetProperty(ref _apikey, value);
                ValidateProperty(value!);
                SubmitCommand.NotifyCanExecuteChanged();
            }
        }


        public ActivateAPIAccessViewModel()
        {
            VisibillityProgressBar = "Hidden";
            VisibillityText = "Hidden";
            SubmitCommand = new AsyncRelayCommand(Submit, () => CanSubmit);

            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            Username = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut);

        }


        private async Task Submit()
        {
            EBFileService eBFileService = new(eBFileManagement);

            await Task.Run( () => {

                VerifyCredentials(Username!, Apikey!);

            });
        }

        public bool CanSubmit => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Apikey);  

        [RelayCommand(CanExecute = nameof(CanExecuteCanSubmitImport))]
        private async Task CanSubmitImport(object value)
        {
            string? filePath = value as string;

            IEBFile eBFileManagement = new EBFileManagement();
            EBFileService eBFileService = new(eBFileManagement);

            await Task.Run( () =>
            {
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);

                    File.Copy(filePath, ISystemPaths.File_Path_API, true);

                    string[]? credentials = eBFileService.ReadAPIFile();

                    if(credentials is null)
                    {

                        VisibillityText = "Visible";
                        RegistrationSuccessfulInfo = "Die Datei ist leer oder fehlerhaft.";
                        ForegroundColorAccessSuccess = "#941616";
                        File.Delete(ISystemPaths.File_Path_API);
                        return;

                    } else
                    {
                        Username = credentials[0];
                        Apikey = credentials[1];

                        VerifyCredentials(Username, Apikey);
                    }                  
                }
            });
        }


        public bool CanExecuteCanSubmitImport(object value)
        {
            if (string.IsNullOrEmpty(FilePathImport))
            {
                return false;

            }
            else { return true; }
        }


        public void VerifyCredentials(string usernamen, string apikey)
        {
            VisibillityProgressBar = "Visible";
            VisibillityText = "Visible";
            RegistrationSuccessfulInfo = "Aktivere Zugang";
            EBFileService eBFileService = new(eBFileManagement);

            string? checkAuthentication = ev2!.AuthenticateInfo(usernamen, apikey);

            if (checkAuthentication.Contains("200"))
            {
                eBFileService.CreateSpecificFileExtension(usernamen, apikey, ISystemPaths.FilePathApplicationOnly, "EB_API_File", ".apiaf");

                string[]? credentials = eBFileService.ReadAPIFile();

                SettingsCache.APIPosition["APIUsername"] = credentials![0];
                SettingsCache.APIPosition["APIKey"] = credentials![1];

                ForegroundColorAccessSuccess = "#169446";
                RegistrationSuccessfulInfo = "eBesucher API erfolgreich aktiviert!";

            }
            else
            {
                ForegroundColorAccessSuccess = "#941616";
                RegistrationSuccessfulInfo = checkAuthentication;
            }

            VisibillityProgressBar = "Hidden";
        }
    }
}
