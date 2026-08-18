using System.Threading.Tasks;

using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Models.FormModels;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

/// <summary>
/// Service interface for presenting modal dialogs, message boxes, and specialized UI prompt dialogs in WinUI 3.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays an informational message dialog asynchronously with an optional status icon.
    /// </summary>
    /// <param name="title">The title header string.</param>
    /// <param name="message">The message body content.</param>
    /// <param name="icon">The status icon to render alongside the title header.</param>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None);

    /// <summary>
    /// Displays a Yes/No decision dialog asynchronously with an optional icon.
    /// </summary>
    /// <param name="title">The title header string.</param>
    /// <param name="message">The decision message content.</param>
    /// <param name="icon">The status icon to render alongside the title header.</param>
    /// <returns><c>true</c> if the user selects Yes (Primary); otherwise, <c>false</c>.</returns>
    Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question);

    /// <summary>
    /// Displays a confirmation dialog asynchronously with customized action button labels.
    /// </summary>
    /// <param name="title">The title header string.</param>
    /// <param name="message">The confirmation message body.</param>
    /// <param name="yesButtonText">The label text for the positive action button.</param>
    /// <param name="noButtonText">The label text for the negative/cancel button.</param>
    /// <returns><c>true</c> if confirmed; otherwise, <c>false</c>.</returns>
    Task<bool> ShowConfirmationAsync(string title, string message, string yesButtonText = "Ja", string noButtonText = "Nein");

    /// <summary>
    /// Displays the Windows auto-logon credentials configuration dialog asynchronously.
    /// </summary>
    /// <param name="defaultUser">Default username value.</param>
    /// <param name="defaultDomain">Default Windows domain or computer name.</param>
    /// <param name="isPasswordlessEnabled">Value indicating whether passwordless sign-in is currently active.</param>
    /// <param name="isAdmin">Value indicating whether the current process possesses elevated administrator privileges.</param>
    /// <returns>An <see cref="AutoLogonDialogResult"/> object if saved or deactivated; otherwise, <c>null</c> if cancelled.</returns>
    Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null!, string defaultDomain = null!, bool isPasswordlessEnabled = false, bool isAdmin = false);

    /// <summary>
    /// Displays the application About information dialog asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowAboutDialogAsync();

    /// <summary>
    /// Displays the browser add-on installation dialog asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowInstallAddOnDialogAsync();

    /// <summary>
    /// Displays the browser add-on info dialog asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowInstallAddOnInfoDialogAsync();

    /// <summary>
    /// Displays the API key activation dialog asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowActivateApiDialogAsync();

    /// <summary>
    /// Displays the browser cache content deletion dialog asynchronously.
    /// </summary>
    /// <param name="shouldAutoStart">If <c>true</c>, automatically executes the deletion sequence upon display.</param>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowDeleteBrowserContentDialogAsync(bool shouldAutoStart = false);

    /// <summary>
    /// Displays the Edge Startup Boost disable prompt dialog asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous dialog presentation.</returns>
    Task ShowTurnOffEdgeStartupBoostDialogAsync();
}
