using System;

namespace eBRestarter.Core.Application.BehavioralComponents.Extensions;

/// <summary>
/// Masks personally identifiable values before they are handed to a log template.
/// </summary>
/// <remarks>
/// <para>
/// <b>Compliance &amp; PII Redaction:</b> Personally identifiable information (PII) such as Windows
/// account names and eBesucher usernames must never appear unmasked in log outputs, regardless of
/// the sink or environment.
/// </para>
/// <para>
/// <b>Source-side Redaction Rationale:</b> Redaction is performed directly at the logging source using
/// standard BCL string operations. This fulfills compliance requirements without pulling in heavy
/// telemetry packaging stacks.
/// </para>
/// <para>
/// <b>Full Masking vs. HMAC Rationale:</b> Full masking is preferred over HMAC tokenization because
/// this single-user application does not require cross-log account correlation, eliminating token
/// reversal risks.
/// </para>
/// <para>
/// <b>Layer Placement:</b> Located in Core because it is required across all architectural layers and
/// relies strictly on BCL string operations without framework dependencies, maintaining Hexagonal boundaries.
/// </para>
/// </remarks>
public static class LogRedaction
{
    private const string EmptyMarker = "(empty)";
    private const string MaskSuffix = "***";
    private const string RedactedSegmentMarker = "***";

    /// <summary>
    /// Masks a user name or comparable identifier so it stays recognisable in shape but not in content.
    /// </summary>
    /// <param name="identifier">The raw identifier. <see langword="null"/> and whitespace are tolerated.</param>
    /// <returns>The first character followed by <c>***</c>, or <c>(empty)</c> when nothing was supplied.</returns>
    /// <example><c>MaskIdentifier("ExampleUser")</c> returns <c>"E***"</c>.</example>
    public static string MaskIdentifier(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return EmptyMarker;
        }

        return string.Concat(identifier.AsSpan(0, 1), MaskSuffix);
    }

    /// <summary>
    /// Replaces the final path segment of a URL, which in this application carries the user name.
    /// </summary>
    /// <param name="url">The raw URL, typically the personalised surfbar address.</param>
    /// <returns>
    /// The URL with its last path segment replaced by <c>***</c>. Unparseable input is masked
    /// entirely rather than passed through — fail secure.
    /// </returns>
    /// <example>
    /// <c>MaskUrlUserSegment("https://www.ebesucher.de/surfbar/Username")</c> returns
    /// <c>"https://www.ebesucher.de/surfbar/***"</c>.
    /// </example>
    public static string MaskUrlUserSegment(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return EmptyMarker;
        }

        if (!Uri.TryCreate(url.Trim('"'), UriKind.Absolute, out Uri? parsedUrl))
        {
            return RedactedSegmentMarker;
        }

        var path = parsedUrl.AbsolutePath;
        var lastSeparatorIndex = path.LastIndexOf('/');

        // If there is no trailing path segment (e.g., path ends with '/'), there is nothing to mask.
        if (lastSeparatorIndex < 0 || lastSeparatorIndex == path.Length - 1)
        {
            return $"{parsedUrl.GetLeftPart(UriPartial.Authority)}{path}";
        }

        var pathWithoutLastSegment = path[..(lastSeparatorIndex + 1)];

        return $"{parsedUrl.GetLeftPart(UriPartial.Authority)}{pathWithoutLastSegment}{RedactedSegmentMarker}";
    }
}
