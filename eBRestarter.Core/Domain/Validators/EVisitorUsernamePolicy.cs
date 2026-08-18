using System.Text.RegularExpressions;

namespace eBRestarter.Core.Domain.Validators;

/// <summary>
/// Single source of truth for what characters an eBesucher username may contain.
/// </summary>
/// <remarks>
/// <para>
/// Input validation policy using strict whitelisting to validate username input.
/// </para>
/// <para>
/// Security relevance: The username is appended to the Surfbar URL and passed as command-line arguments to the browser process.
/// Strict whitelisting prevents command-line argument injection (e.g., injecting additional Chromium flags).
/// Allowed characters exclude spaces, quotes, leading hyphen flags, and path separators.
/// </para>
/// <para>
/// Part of defense-in-depth: Username validation is complemented by URL encoding and shell argument quoting during launch.
/// </para>
/// </remarks>
public static partial class EVisitorUsernamePolicy
{
    private const int MaximumLength = 64;

    /// <summary>Human-readable description of the rule, suitable for validation messages.</summary>
    public const string RuleDescription = "The username may only contain letters, digits, dots, hyphens and underscores (1–64 characters).";

    /// <summary>
    /// Determines whether the given username satisfies the whitelist.
    /// </summary>
    /// <param name="username">The candidate username. <see langword="null"/> and whitespace are rejected.</param>
    /// <returns><c>true</c> if the username satisfies length and character whitelist constraints; otherwise, <c>false</c>.</returns>
    public static bool IsValid(string? username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > MaximumLength)
        {
            return false;
        }

        return AllowedUsernameRegex().IsMatch(username);
    }

    /// <summary>
    /// Anchored whitelist regex. Uses <see cref="RegexOptions.NonBacktracking"/> to prevent catastrophic backtracking.
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z0-9._-]+$", RegexOptions.NonBacktracking, matchTimeoutMilliseconds: 100)]
    private static partial Regex AllowedUsernameRegex();
}
