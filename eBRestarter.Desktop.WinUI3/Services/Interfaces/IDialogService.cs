using eBRestarter.Desktop.WinUI3.Models;
using System.Threading.Tasks;
using eBRestarter.Desktop.WinUI3.Models.Enums;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None);
    Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question);
    Task<bool> ShowConfirmationAsync(string title, string message, string yesButtonText = "Ja", string noButtonText = "Nein");
    Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null!, string defaultDomain = null!);
    Task ShowAboutDialogAsync();
    Task ShowInstallAddOnDialogAsync();
    Task ShowInstallAddOnInfoDialogAsync();
    Task ShowActivateApiDialogAsync();
    Task ShowDeleteBrowserContentDialogAsync(bool autoStart = false);
    Task ShowTurnOffEdgeStartupBoostDialogAsync();
}
