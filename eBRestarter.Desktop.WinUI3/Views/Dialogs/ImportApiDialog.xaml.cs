using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public sealed partial class ImportApiDialog : ContentDialog
{
    public ViewModelImportApi ViewModelImportApi { get; }

    public ImportApiDialog()
    {
        InitializeComponent();
        ViewModelImportApi = App.AppHost!.Services.GetRequiredService<ViewModelImportApi>();
    }

    // --- Drag & Drop ---
    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "Hier ablegen";
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0 && items[0] is Windows.Storage.StorageFile file)
            {
                ViewModelImportApi.HandleFileDrop(file.Path);
            }
        }
    }

    // --- File Picker ---
    private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        //// WinUI 3 Window Handle Hack (notwendig für Picker)
        //var window = App.MainWindoweBRestarter; // Dein Main Window Property

        //var picker = new FileOpenPicker(window);
        //var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        //WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

        //picker.ViewMode = PickerViewMode.Thumbnail;
        //picker.SuggestedStartLocation = PickerLocationId.Desktop;
        //picker.FileTypeFilter.Add(".apiaf"); // Filter auf deine Extension

        //var file = await picker.PickSingleFileAsync();
        //if (file != null)
        //{
        //    ViewModel.HandleFileDrop(file.Path);
        //}
    }
}
