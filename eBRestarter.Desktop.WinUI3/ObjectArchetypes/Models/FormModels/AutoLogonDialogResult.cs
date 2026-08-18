using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.Models.FormModels;

/// <summary>
/// Represents the result data originating from an auto-logon configuration dialog interaction.
/// </summary>
public sealed class AutoLogonDialogResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the deactivate action was selected.
    /// </summary>
    public bool IsDeactivateAction { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether passwordless mode should be disabled.
    /// </summary>
    public bool DisablePasswordlessMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether passwordless mode should be reactivated.
    /// </summary>
    public bool RestorePasswordlessMode { get; set; }

    /// <summary>
    /// Gets or sets the auto-logon credentials payload (only populated when saving credentials).
    /// </summary>
    public AutoLogonCredentials? Credentials { get; set; }

    /// <summary>
    /// Creates a dialog result configured for deactivating auto-logon settings.
    /// </summary>
    /// <param name="restorePasswordlessMode"><see langword="true"/> to restore passwordless mode; otherwise, <see langword="false"/>.</param>
    /// <returns>A new <see cref="AutoLogonDialogResult"/> instance representing deactivation.</returns>
    public static AutoLogonDialogResult Deactivate(bool restorePasswordlessMode = false) => new()
    {
        IsDeactivateAction = true,
        RestorePasswordlessMode = restorePasswordlessMode
    };

    /// <summary>
    /// Creates a dialog result configured for saving new auto-logon credentials.
    /// </summary>
    /// <param name="creds">The auto-logon credentials to store.</param>
    /// <param name="disablePasswordlessMode"><see langword="true"/> to disable passwordless mode; otherwise, <see langword="false"/>.</param>
    /// <returns>A new <see cref="AutoLogonDialogResult"/> instance containing the updated credentials.</returns>
    public static AutoLogonDialogResult Save(AutoLogonCredentials creds, bool disablePasswordlessMode = false) => new()
    {
        IsDeactivateAction = false,
        Credentials = creds,
        DisablePasswordlessMode = disablePasswordlessMode
    };

    /// <summary>
    /// Returns a cancelled dialog result representation.
    /// </summary>
    /// <returns><see langword="null"/> indicating dialog cancellation.</returns>
    public static AutoLogonDialogResult? Cancel() => null;
}