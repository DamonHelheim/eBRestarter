using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Text.RegularExpressions;

namespace eBRestarter.Desktop.WinUI3.Behaviors;

/// <summary>
/// Attached behavior that restricts a <see cref="TextBox"/> to numeric input only.
/// </summary>
public class TextBoxNumericOnlyBehavior : Behavior<TextBox>
{
    // =========================================================
    // 1. CONSTANTS & STATICS (Shared regex for non-digit stripping)
    // =========================================================
    #region ConstantsAndStatics

    // SonarQube Fix: Timeout (100ms) und NonBacktracking hinzugefügt, um UI-Freezes durch ReDoS zu verhindern.
    private static readonly Regex _regex = new("[^0-9]+", RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(100));

    #endregion

    // =========================================================
    // 2. PROTECTED METHODS (Behavior lifecycle overrides)
    // =========================================================
    #region ProtectedMethods

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.TextChanging += OnTextChanging;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.TextChanging -= OnTextChanging;
    }

    #endregion

    // =========================================================
    // 3. PRIVATE HELPER METHODS (Event handlers and helpers)
    // =========================================================
    #region PrivateHelperMethods

    private void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        try
        {
            if (_regex.IsMatch(sender.Text))
            {
                int pos = sender.SelectionStart;
                sender.Text = _regex.Replace(sender.Text, "");
                sender.SelectionStart = System.Math.Min(pos, sender.Text.Length);
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Fallback: Falls der Text wirklich zu extrem ist und das Regex abbricht,
            // fangen wir den Fehler ab, damit die App nicht abstürzt.
            sender.Text = string.Empty;
        }
    }

    #endregion
}