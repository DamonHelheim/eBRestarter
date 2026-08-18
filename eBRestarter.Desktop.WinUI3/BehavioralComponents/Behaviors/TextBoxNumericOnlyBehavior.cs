using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Behaviors;

/// <summary>
/// Attached behavior that restricts a <see cref="TextBox"/> to numeric input only.
/// </summary>
public sealed class TextBoxNumericOnlyBehavior : Behavior<TextBox>
{
    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.TextChanging += OnTextChanging;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.TextChanging -= OnTextChanging;
    }

    private static void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        string currentText = sender.Text;

        if (string.IsNullOrEmpty(currentText))
        {
            return;
        }

        ReadOnlySpan<char> span = currentText.AsSpan();

        if (!HasNonDigits(span))
        {
            return;
        }

        int originalSelectionStart = sender.SelectionStart;

        string cleanText = RemoveNonDigits(span);

        sender.Text = cleanText;
        sender.SelectionStart = Math.Min(originalSelectionStart, cleanText.Length);
    }

    /// <summary>
    /// Checks whether the span contains any non-digit characters without allocating memory.
    /// </summary>
    /// <param name="span">The character span to inspect.</param>
    /// <returns><c>true</c> if any non-digit character is found; otherwise, <c>false</c>.</returns>
    private static bool HasNonDigits(ReadOnlySpan<char> span)
    {
        foreach (char c in span)
        {
            if (!char.IsAsciiDigit(c))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes non-digit characters using allocation-efficient string.Create.
    /// </summary>
    /// <param name="source">The source character span containing text input.</param>
    /// <returns>A new string containing only ASCII digit characters.</returns>
    private static string RemoveNonDigits(ReadOnlySpan<char> source)
    {
        int digitCount = 0;
        foreach (char c in source)
        {
            if (char.IsAsciiDigit(c))
            {
                digitCount++;
            }
        }

        if (digitCount == 0)
        {
            return string.Empty;
        }

        return string.Create(digitCount, source, static (destination, span) =>
        {
            int index = 0;
            foreach (char c in span)
            {
                if (char.IsAsciiDigit(c))
                {
                    destination[index++] = c;
                }
            }
        });
    }
}