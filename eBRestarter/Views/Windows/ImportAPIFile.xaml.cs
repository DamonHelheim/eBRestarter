using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Classes.Cache;
using eBRestarter.Classes.EBEvents;

namespace eBRestarter.Views.Windows
{

    public partial class ImportAPIFile : MetroWindow
    {
        private ActivateAPIAccessViewModel ActivateAPIAccessViewModel { get; } = new();

        private readonly UserNameEnteredEvent? nameEnteredEvent;

        public ImportAPIFile(UserNameEnteredEvent? nameEnteredEvent)
        {
            InitializeComponent();

            this.DataContext = ActivateAPIAccessViewModel;
            this.nameEnteredEvent = nameEnteredEvent;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (SettingsCache.APIPosition["APIUsername"] is not null && SettingsCache.APIPosition["APIKey"] is not null)
            {
                nameEnteredEvent!.RaiseNameEnteredEvent("Nutzername: " + SettingsCache.APIPosition["APIUsername"]);
            }
            else
            {
                nameEnteredEvent!.RaiseNameEnteredEvent("Nutzername: ");
            }
        }

        private void BtnImportLicenseData_Click(object sender, RoutedEventArgs e)
        {
            var filePath = string.Empty;

            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "eB API File (*.apiaf)|*.apiaf",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;
                BtnAPIFileImport.Visibility = Visibility.Visible;
            }

            tb_filePath.Text = filePath;
        }

        private void FileDropStackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string filename = System.IO.Path.GetFileName(files[0]);

                string fullFilePath = System.IO.Path.GetFullPath(files[0]);

                string fileExtension = System.IO.Path.GetFullPath(files[0]);

                if (!fileExtension.Contains(".apiaf"))
                {
                    tbl_FileName.Text = "Datei enthält ein ungültiges Dateiformat!";
                    tb_filePath.Text = string.Empty;
                    im_dataFile.Source = new BitmapImage(new Uri(@"\Resources\Images\Icons\Intersection\wrong_document.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    im_dataFile.Source = new BitmapImage(new Uri(@"\Resources\Images\Icons\Intersection\approval.png", UriKind.RelativeOrAbsolute));
                    tbl_FileName.Text = filename;
                    tb_filePath.Text = fullFilePath;
                    BtnAPIFileImport.Visibility = Visibility.Visible;
                }
            }
        }    
    }
}
