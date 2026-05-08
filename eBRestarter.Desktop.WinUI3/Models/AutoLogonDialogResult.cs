using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Desktop.WinUI3.Models;
public sealed class AutoLogonDialogResult
{
    public bool IsDeactivateAction { get; set; } // Wurde "Deaktivieren" geklickt?
    public AutoLogonCredentials? Credentials { get; set; } // Daten (nur bei Speichern)

    // Statische Helper für sauberen Code
    public static AutoLogonDialogResult Deactivate() => new() { IsDeactivateAction = true };
    public static AutoLogonDialogResult Save(AutoLogonCredentials creds) => new() { IsDeactivateAction = false, Credentials = creds };
    public static AutoLogonDialogResult? Cancel() => null; // Einfach null zurückgeben bei Abbruch
}
