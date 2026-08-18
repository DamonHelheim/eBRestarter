using System;

using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.BehavioralComponents.Logging;

/// <summary>
/// Source-generated log messages for the browser cache cleanup path.
/// </summary>
/// <remarks>
/// <para>
/// 📝 Logging Guidelines Section 3, <b>Pattern A (static extension methods)</b>. This pattern —
/// rather than Pattern B — is required here because the callers in
/// <c>AdapterWindowsFileDeletionService</c> are <c>static</c> and receive the
/// <see cref="ILogger"/> as a parameter; instance-based partial methods under Pattern B require an injected logger field.
/// </para>
/// <para>
/// <b>Rationale for high-performance optimization:</b> Section 3 mandates source-generated logging
/// for high-throughput code paths. File cleanup routinely processes 50,000–200,000 files in an inner loop.
/// The source generator places the <c>IsEnabled</c> check prior to argument evaluation, eliminating value-type boxing,
/// <c>object[]</c> allocations, and runtime template parsing overhead per invocation.
/// </para>
/// </remarks>
public static partial class FileDeletionLogMessages
{
    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCacheDirectoryUnreadable,
        Level = LogLevel.Debug,
        Message = "Error while gathering files from directory: {Directory}")]
    public static partial void LogDirectoryEnumerationFailed(
        this ILogger logger,
        Exception exception,
        string directory);

    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCacheDirectoryNotRemoved,
        Level = LogLevel.Debug,
        Message = "Directory could not be deleted (it might not be empty): {Directory}")]
    public static partial void LogDirectoryNotRemoved(
        this ILogger logger,
        Exception exception,
        string directory);

    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCacheFileDeletionFailed,
        Level = LogLevel.Debug,
        Message = "Error while deleting file payload: {File}")]
    public static partial void LogFileDeletionFailed(
        this ILogger logger,
        Exception exception,
        string file);

    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCacheFileDeletionFailed,
        Level = LogLevel.Warning,
        Message = "Error while deleting individual file: {File}")]
    public static partial void LogSingleFileDeletionFailed(
        this ILogger logger,
        Exception exception,
        string file);

    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCacheReparsePointSkipped,
        Level = LogLevel.Debug,
        Message = "Skipping reparse point (junction/symlink) {Directory} during cleanup to avoid deleting outside the cleanup root.")]
    public static partial void LogReparsePointSkipped(
        this ILogger logger,
        string directory);

    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCachePathOutsideRoot,
        Level = LogLevel.Warning,
        Message = "Refusing to delete {File}: the resolved path lies outside the requested cleanup root {CleanupRoot}.")]
    public static partial void LogPathOutsideCleanupRoot(
        this ILogger logger,
        string file,
        string cleanupRoot);

    [LoggerMessage(
        EventId = LogEventIds.Browser.BrowserCachePathOutsideRoot,
        Level = LogLevel.Debug,
        Message = "Could not resolve {File} against the cleanup root {CleanupRoot}; the file was skipped.")]
    public static partial void LogPathResolutionFailed(
        this ILogger logger,
        Exception exception,
        string file,
        string cleanupRoot);
}
