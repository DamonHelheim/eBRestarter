using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Behaviors;

/// <summary>
/// Attached behavior that restricts a <see cref="TextBox"/> to numeric input only.
/// </summary>
public sealed class TextBoxNumericOnlyBehavior : Behavior<TextBox>
{
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

    // ✅ .NET 10: Static handler prevents delegate instance state capturing.
    private static void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        string currentText = sender.Text;
        if (string.IsNullOrEmpty(currentText))
        {
            return;
        }

        // ✅ C# 14 / .NET 10: Allocation-free Span check (0 Bytes allocated on valid input).
        ReadOnlySpan<char> span = currentText.AsSpan();
        if (!HasNonDigits(span))
        {
            return;
        }

        int originalSelectionStart = sender.SelectionStart;

        // ✅ .NET 10: Single-pass zero-intermediate-allocation string creation.
        string cleanText = RemoveNonDigits(span);

        sender.Text = cleanText;
        sender.SelectionStart = Math.Min(originalSelectionStart, cleanText.Length);
    }

    /// <summary>
    /// Checks whether the span contains any non-digit characters without allocating memory.
    /// </summary>
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