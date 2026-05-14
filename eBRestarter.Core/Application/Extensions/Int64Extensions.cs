namespace eBRestarter.Core.Application.Extensions;

using System;
using System.Globalization;

public static class Int64Extensions
{
    // ZB und YB entfernt, da ein Int64 (long) maximal ~9.2 EB fassen kann.
    // "bytes" zu "B" geändert für eine einheitliche Optik.
    private static readonly string[] _sizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    /// <summary>
    /// Formatiert eine Byte-Zahl in einen lesbaren String (z.B. 1.5 MB).
    /// </summary>
    /// <param name="value">Die Größe in Bytes.</param>
    /// <param name="decimalPlaces">Anzahl der Nachkommastellen (Standard: 1).</param>
    /// <param name="formatProvider">Die zu verwendende Kultur (Standard: Aktuelle Systemkultur).</param>
    public static string ToSizeSuffix(this long value, int decimalPlaces = 1, IFormatProvider? formatProvider = null)
    {
        // Provider festlegen (falls null, nutze aktuelle Kultur)
        formatProvider ??= CultureInfo.CurrentCulture;

        if (value < 0) { return "-" + ToSizeSuffix(-value, decimalPlaces, formatProvider); }
        if (value == 0) { return "0 B"; }

        int mag = (int)Math.Log(value, 1024);

        // Schutz vor IndexOutOfRange (obwohl Math.Log bei long max. 6 liefert)
        if (mag >= _sizeSuffixes.Length)
        {
            mag = _sizeSuffixes.Length - 1;
        }

        // Bytes brauchen keine Nachkommastellen
        if (mag == 0)
        {
            return $"{value} {_sizeSuffixes[0]}";
        }

        // Math.Pow ist sicherer als Bitshift bei variablen Größen und direkt für double/decimal geeignet
        decimal adjustedSize = (decimal)(value / Math.Pow(1024, mag));

        // Dynamisches String-Format basierend auf decimalPlaces aufbauen (z.B. "n1", "n2")
        string numberFormat = $"n{decimalPlaces}";

        // IFormatProvider nutzen, um Probleme mit Punkt/Komma auf internationalen Systemen zu vermeiden
        return $"{adjustedSize.ToString(numberFormat, formatProvider)} {_sizeSuffixes[mag]}";
    }
}
