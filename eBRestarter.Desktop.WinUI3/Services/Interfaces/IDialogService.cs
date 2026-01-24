using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{

    public interface IDialogService
    {
        // Standard dialogs (with optional icon)
        Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None);
        Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question);

        // Custom dialogs (simplified signatures)
        Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null);
        Task ShowAboutDialogAsync();
        Task ShowBrowserDeleteContentDialogAsync();
        Task ShowInstallAddOnDialogAsync();
        Task ShowActivateApiDialogAsync();
        Task ShowImportApiDialogAsync();
    }


    //public interface IDialogService
    //{
    //    // Icon ist optional (= DialogIcon.None), damit alter Code nicht kaputt geht
    //    Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None);
    //    Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question);

    //    // ... deine restlichen Methoden bleiben gleich ...
    //    Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null);
    //    Task ShowAboutDialogAsync();
    //    Task ShowBrowserDeleteContentDialogAsync();
    //    Task ShowInstallAddOnDialogAsync();
    //    Task ShowActivateApiDialogAsync();
    //    Task ShowImportApiDialogAsync();
    //}
}
