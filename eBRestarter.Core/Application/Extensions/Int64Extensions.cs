namespace eBRestarter.Core.Application.Extensions;

using System;
using System.Globalization;

public static class Int64Extensions
{
    // ZB and YB removed because an Int64 (long) can only hold a maximum of ~9.2 EB.
    // Changed "bytes" to "B" for a consistent look.
    private static readonly string[] _sizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    /// <summary>
    /// Formats a byte value into a human-readable string (e.g., 1.5 MB).
    /// </summary>
    /// <param name="value">The size in bytes.</param>
    /// <param name="decimalPlaces">The number of decimal places (default: 1).</param>
    /// <param name="formatProvider">The culture to use (default: current system culture).</param>
    public static string ToSizeSuffix(this long value, int decimalPlaces = 1, IFormatProvider? formatProvider = null)
    {
        // Set provider (if null, use current culture)
        formatProvider ??= CultureInfo.CurrentCulture;

        if (value < 0) { return "-" + ToSizeSuffix(-value, decimalPlaces, formatProvider); }
        if (value == 0) { return "0 B"; }

        int mag = (int)Math.Log(value, 1024);

        // Guard against IndexOutOfRange (even though Math.Log returns max 6 for long)
        if (mag >= _sizeSuffixes.Length)
        {
            mag = _sizeSuffixes.Length - 1;
        }

        // Bytes do not require decimal places
        if (mag == 0)
        {
            return $"{value} {_sizeSuffixes[0]}";
        }

        // Math.Pow is safer than bitwise shifting for variable sizes and directly compatible with double/decimal
        decimal adjustedSize = (decimal)(value / Math.Pow(1024, mag));

        // Build dynamic string format based on decimalPlaces (e.g., "n1", "n2")
        string numberFormat = $"n{decimalPlaces}";

        // Use IFormatProvider to prevent issues with dot/comma formatting on international systems
        return $"{adjustedSize.ToString(numberFormat, formatProvider)} {_sizeSuffixes[mag]}";
    }
}