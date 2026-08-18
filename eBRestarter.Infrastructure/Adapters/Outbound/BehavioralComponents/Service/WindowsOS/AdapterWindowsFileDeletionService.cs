using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces;
using eBRestarter.Infrastructure.BehavioralComponents.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Service.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for deleting temporary files and browser cache folders on Windows.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Cleans up temporary filesystem assets and browser caches in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortFileDeletion"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsFileDeletionService : IOutboundPortFileDeletion
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string AllFilesSearchPattern = "*";
    private const int DefaultReportInterval = 10;
    private const string DeletingFileStatusFormat = "Deleting: {0}";
    private const string FinalizingCleanupStatusMessage = "Finalizing cleanup operations...";
    private const int InitialCounterValue = 0;
    private const string ProtectedFirefoxExtensionFilter = "moz-extension";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly ILogger<AdapterWindowsFileDeletionService> _logger;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterWindowsFileDeletionService"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public AdapterWindowsFileDeletionService(ILogger<AdapterWindowsFileDeletionService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Deletes the contents of every given directory in a single recursive pass.
    /// </summary>
    /// <param name="directories">List of directory paths to clean up.</param>
    /// <param name="statusReporter">Progress reporter for user-facing status messages.</param>
    /// <param name="completedDirectoriesReporter">Progress reporter for completed directory count.</param>
    /// <param name="token">Cancellation token.</param>
    /// <remarks>
    /// Performance optimization: Performs directory cleanup in a single recursive pass, reporting progress on directory completion without requiring pre-enumeration file counts.
    /// </remarks>
    public Task DeleteFilesAsync(List<string> directories, IProgress<string> statusReporter, IProgress<int> completedDirectoriesReporter, CancellationToken token)
    {
        // ⚡ Immediate Guard-Clause Exception Timing & Direct Task Return (Guide Abs. 9.1 & 15)
        ArgumentNullException.ThrowIfNull(directories);
        ArgumentNullException.ThrowIfNull(statusReporter);
        ArgumentNullException.ThrowIfNull(completedDirectoriesReporter);

        return Task.Run(() =>
        {
            var context = new DeletionContext
            {
                DeletedCount = InitialCounterValue,
                UpdateCounter = InitialCounterValue,
                ReportInterval = DefaultReportInterval,
                StatusReporter = statusReporter,
                Token = token
            };

            int completedDirectories = InitialCounterValue;

            foreach (var dir in directories)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                ProcessDirectory(dir, context, _logger);

                completedDirectories++;
                completedDirectoriesReporter.Report(completedDirectories);
            }

            // AT THE END: Dispatch a final progress update so the bar reaches 100 % even if the
            // last directory was skipped or unreadable.
            statusReporter.Report(FinalizingCleanupStatusMessage);
            completedDirectoriesReporter.Report(directories.Count);

        }, token);
    }

    /// <summary>
    /// Deletes a single file at the specified path if it exists.
    /// </summary>
    /// <param name="filePath">Target file path.</param>
    public void DeleteSingleFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception exception)
            {
                _logger.LogSingleFileDeletionFailed(exception, filePath);
            }
        }
    }

    /// <summary>
    /// Processes and deletes files within a directory while enforcing path safety boundaries.
    /// </summary>
    /// <param name="dir">Target directory path.</param>
    /// <param name="context">Deletion context tracking counters and reporters.</param>
    /// <param name="logger">Logger instance.</param>
    /// <remarks>
    /// Security &amp; Path Traversal Prevention: Skips reparse points (symlinks/junctions) and validates resolved canonical file paths against the cleanup root to prevent directory traversal outside cache boundaries.
    /// </remarks>
    private static void ProcessDirectory(string dir, DeletionContext context, ILogger logger)
    {
        if (!Directory.Exists(dir))
        {
            return;
        }

        if (IsReparsePoint(dir))
        {
            logger.LogReparsePointSkipped(dir);

            return;
        }

        string cleanupRoot;
        IEnumerable<string> files;

        try
        {
            // Canonical root with trailing separator to prevent partial directory name matching.
            cleanupRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dir)) + Path.DirectorySeparatorChar;

            // ✅ .NET 10: EnumerateFiles vermeidet Allokation riesiger string[]-Arrays im RAM bei großen Caches
            files = Directory.EnumerateFiles(dir, AllFilesSearchPattern, SearchOption.AllDirectories);
        }
        catch (Exception exception)
        {
            logger.LogDirectoryEnumerationFailed(exception, dir);

            return;
        }

        foreach (var file in files)
        {
            if (context.Token.IsCancellationRequested)
            {
                return;
            }

            if (!IsInsideCleanupRoot(file, cleanupRoot, logger))
            {
                continue;
            }

            ProcessFile(file, context, logger);
        }

        // Delete directory node
        if (context.Token.IsCancellationRequested)
        {
            return;
        }

        // The parameter setup evaluates recursive: false. Thus, the folder is ONLY deleted if it is completely empty.
        // If system configurations or specialized files like "moz-extension" remain, the parent directory container will persist.
        try
        {
            Directory.Delete(dir, recursive: false);
        }
        catch (Exception exception)
        {
            logger.LogDirectoryNotRemoved(exception, dir);
        }
    }

    /// <summary>
    /// Verifies that the fully resolved file path still lies beneath the requested cleanup root.
    /// </summary>
    /// <param name="file">File path to verify.</param>
    /// <param name="cleanupRoot">Allowed canonical cleanup root path.</param>
    /// <param name="logger">Logger instance.</param>
    private static bool IsInsideCleanupRoot(string file, string cleanupRoot, ILogger logger)
    {
        try
        {
            var resolvedPath = Path.GetFullPath(file);

            if (resolvedPath.StartsWith(cleanupRoot, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            logger.LogPathOutsideCleanupRoot(file, cleanupRoot);
        }
        catch (Exception exception)
        {
            // Fail Secure: If path cannot be resolved, skip deletion.
            logger.LogPathResolutionFailed(exception, file, cleanupRoot);
        }

        return false;
    }

    /// <summary>
    /// Determines whether the path is a junction or symbolic link whose target may point outside
    /// the cleanup root.
    /// </summary>
    /// <param name="path">Filesystem item path.</param>
    private static bool IsReparsePoint(string path)
    {
        try
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
        }
        catch (Exception)
        {
            // Fail Secure: Treat ambiguous paths as reparse points to skip.
            return true;
        }
    }

    /// <summary>
    /// Deletes an individual file if not protected by filter criteria.
    /// </summary>
    /// <param name="file">File path to process.</param>
    /// <param name="context">Deletion context tracking progress.</param>
    /// <param name="logger">Logger instance.</param>
    private static void ProcessFile(string file, DeletionContext context, ILogger logger)
    {
        try
        {
            // Do not delete protected engine assets like specific Firefox components
            if (file.Contains(ProtectedFirefoxExtensionFilter, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            File.Delete(file);
            context.DeletedCount++;
            context.UpdateCounter++;

            // Only dispatch status updates when the defined 'ReportInterval' window has passed.
            // Keeps UI status text active during batch file operations.
            if (context.UpdateCounter >= context.ReportInterval)
            {
                string statusMessage = string.Format(DeletingFileStatusFormat, Path.GetFileName(file));
                context.StatusReporter.Report(statusMessage);
                context.UpdateCounter = InitialCounterValue; // Reset
            }
        }
        catch (Exception exception)
        {
            logger.LogFileDeletionFailed(exception, file);
        }
    }


    // ═══════════════════════════════════════════════════════
    //  9. Nested Types
    // ═══════════════════════════════════════════════════════
    private sealed class DeletionContext
    {
        public int DeletedCount { get; set; }
        public int UpdateCounter { get; set; }
        public int ReportInterval { get; set; }
        public IProgress<string> StatusReporter { get; set; } = null!;
        public CancellationToken Token { get; set; }
    }
}