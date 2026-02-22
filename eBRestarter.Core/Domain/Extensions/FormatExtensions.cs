namespace eBRestarter.Core.Domain.Extensions;

public static class FormatExtensions
{
    private static readonly string[] _sizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

    public static string ToSizeSuffix(this long value)
    {
        if (value < 0) { return "-" + ToSizeSuffix(-value); }
        if (value == 0) { return "0.0 bytes"; }

        int mag = (int)Math.Log(value, 1024);

        // Schutz vor Überlauf bei extrem großen Zahlen
        if (mag >= _sizeSuffixes.Length)
        {
            mag = _sizeSuffixes.Length - 1;
        }

        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // Nutzt String Interpolation (moderner als string.Format)
        return $"{adjustedSize:n1} {_sizeSuffixes[mag]}";
    }
}
