using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Text.RegularExpressions;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Behaviors;

/// <summary>
/// Attached behavior that restricts a <see cref="TextBox"/> to numeric input only.
/// </summary>
public sealed class TextBoxNumericOnlyBehavior : Behavior<TextBox>
{
    // SonarQube Fix: Added a timeout (100ms) and NonBacktracking to prevent UI freezes caused by ReDoS.
    private static readonly Regex _regex = new("[^0-9]+", RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(100));

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

    private void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        try
        {
            if (_regex.IsMatch(sender.Text))
            {
                int pos = sender.SelectionStart;
                sender.Text = _regex.Replace(sender.Text, "");
                sender.SelectionStart = Math.Min(pos, sender.Text.Length);
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Fallback: If the text is extremely malformed and the regex times out,
            // we catch the exception to prevent the application from crashing.
            sender.Text = string.Empty;
        }
    }
}