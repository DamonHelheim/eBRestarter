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
        // =========================================================
        // 1. PUBLIC METHODS (API / Contract)
        // =========================================================
        #region PublicMethods

        Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None);
        Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question);
        Task<bool> ShowConfirmationAsync(string title, string message, string yesButtonText = "Ja", string noButtonText = "Nein");
        Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null);
        Task ShowAboutDialogAsync();
        Task ShowInstallAddOnDialogAsync();
        Task ShowInstallAddOnInfoDialogAsync();
        Task ShowActivateApiDialogAsync();
        Task ShowImportApiDialogAsync();
        Task ShowDeleteBrowserContentDialogAsync(bool autoStart = false);
        Task ShowTurnOffEdgeStartupBoostDialogAsync();

        #endregion
    }
}
