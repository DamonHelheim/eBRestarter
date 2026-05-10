using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Desktop.WinUI3.Models;
public sealed class AutoLogonDialogResult
{
    public bool IsDeactivateAction { get; set; } // Wurde "Deaktivieren" geklickt?
    public bool DisablePasswordlessMode { get; set; } // Soll Passwordless deaktiviert werden?
    public bool RestorePasswordlessMode { get; set; } // Soll Passwordless reaktiviert werden?
    public AutoLogonCredentials? Credentials { get; set; } // Daten (nur bei Speichern)

    // Statische Helper für sauberen Code
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
    
    public static AutoLogonDialogResult? Cancel() => null; // Einfach null zurückgeben bei Abbruch
}
