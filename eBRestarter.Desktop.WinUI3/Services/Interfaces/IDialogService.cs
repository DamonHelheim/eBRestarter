using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None);
    Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question);
    Task<bool> ShowConfirmationAsync(string title, string message, string yesButtonText = "Ja", string noButtonText = "Nein");
    Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null!, string defaultDomain = null!, bool isPasswordlessEnabled = false, bool isAdmin = false);
    Task ShowAboutDialogAsync();
    Task ShowInstallAddOnDialogAsync();
    Task ShowInstallAddOnInfoDialogAsync();
    Task ShowActivateApiDialogAsync();
    Task ShowDeleteBrowserContentDialogAsync(bool autoStart = false);
    Task ShowTurnOffEdgeStartupBoostDialogAsync();
}
