using System.Globalization;

namespace eBRestarter.Core.Application.BehavioralComponents.Extensions;

/// <summary>
/// Extension methods for <see cref="long"/> (Int64) formatting and conversions.
/// </summary>
public static class Int64Extensions
{
    private const int ByteMagnitudeBase = 1024;
    private const string NegativeSign = "-";
    private const string ZeroBytesFormatted = "0 B";

    private static readonly double[] _magnitudeDivisors =
    [
        1d,
        1024d,
        1048576d,
        1073741824d,
        1099511627776d,
        1125899906842624d,
        1152921504606846976d
    ];

    private static readonly string[] _numberFormats = ["n0", "n1", "n2", "n3", "n4", "n5", "n6"];

    // ZB and YB removed because an Int64 (long) can only hold a maximum of ~9.2 EB.
    private static readonly string[] _sizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    /// <summary>
    /// Formats a byte value into a human-readable string (e.g., 1.5 MB).
    /// </summary>
    /// <param name="value">The size in bytes.</param>
    /// <param name="decimalPlaces">The number of decimal places (default: 1).</param>
    /// <param name="formatProvider">The culture to use (default: current system culture).</param>
    public static string ToSizeSuffix(this long value, int decimalPlaces = 1, IFormatProvider? formatProvider = null)
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        if (value < 0)
            return NegativeSign + ToSizeSuffix(-value, decimalPlaces, formatProvider);

        if (value == 0)
            return ZeroBytesFormatted;

        // Guard against IndexOutOfRange (even though Math.Log returns max 6 for long)
        int magnitudeIndex = Math.Min((int)Math.Log(value, ByteMagnitudeBase), _sizeSuffixes.Length - 1);

        // Bytes do not require decimal places
        if (magnitudeIndex == 0)
        {
            return $"{value} {_sizeSuffixes[0]}";
        }

        double adjustedSize = value / _magnitudeDivisors[magnitudeIndex];

        int effectiveDecimalPlaces = Math.Clamp(decimalPlaces, 0, _numberFormats.Length - 1);
        string numberFormat = _numberFormats[effectiveDecimalPlaces];

        adjustedSize = Math.Round(adjustedSize, effectiveDecimalPlaces, MidpointRounding.AwayFromZero);

        // Use IFormatProvider to prevent issues with dot/comma formatting on international systems
        return $"{adjustedSize.ToString(numberFormat, formatProvider)} {_sizeSuffixes[magnitudeIndex]}";
    }
}