using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

public sealed partial class AutoLogonDialog : ContentDialog
{
    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    public bool DisablePasswordlessMode => CbDisablePasswordless.IsChecked ?? false;
    public bool RestorePasswordlessMode => CbRestorePasswordless.IsChecked ?? false;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AutoLogonDialog()
    {
        InitializeComponent();
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    public AutoLogonCredentials GetCredentials()
    {
        return new AutoLogonCredentials(TxtUsername.Text, TxtDomain.Text, PbPassword.Password);
    }

    public void SetDefaults(
        string username,
        string domain,
        bool isPasswordlessEnabled = false,
        bool isAdmin = false)
    {
        if (!string.IsNullOrEmpty(username))
        {
            TxtUsername.Text = username;
        }

        if (!string.IsNullOrEmpty(domain))
        {
            TxtDomain.Text = domain;
        }

        if (!isPasswordlessEnabled)
        {
            if (isAdmin)
            {
                CbRestorePasswordless.Visibility = Visibility.Visible;
            }

            return;
        }

        if (!isAdmin)
        {
            AdminWarningInfoBar.IsOpen = true;
            IsPrimaryButtonEnabled = false;

            return;
        }

        CbDisablePasswordless.Visibility = Visibility.Visible;
        CbDisablePasswordless.IsChecked = true;
    }
}
