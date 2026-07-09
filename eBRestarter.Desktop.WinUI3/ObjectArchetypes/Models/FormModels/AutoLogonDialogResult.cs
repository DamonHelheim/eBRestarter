using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.Models.FormModels;

public sealed class AutoLogonDialogResult
{
    public bool IsDeactivateAction { get; set; } // Was "Deactivate" clicked?
    public bool DisablePasswordlessMode { get; set; } // Should Passwordless mode be disabled?
    public bool RestorePasswordlessMode { get; set; } // Should Passwordless mode be reactivated?
    public AutoLogonCredentials? Credentials { get; set; } // Data (only populated on save)

    // Static helpers for clean code
    public static AutoLogonDialogResult Deactivate(bool restorePasswordlessMode = false) => new()
    {
        IsDeactivateAction = true,
        RestorePasswordlessMode = restorePasswordlessMode
    };

    public static AutoLogonDialogResult Save(AutoLogonCredentials creds, bool disablePasswordlessMode = false) => new()
    {
        IsDeactivateAction = false,
        Credentials = creds,
        DisablePasswordlessMode = disablePasswordlessMode
    };

    public static AutoLogonDialogResult? Cancel() => null; // Simply return null on cancellation
}