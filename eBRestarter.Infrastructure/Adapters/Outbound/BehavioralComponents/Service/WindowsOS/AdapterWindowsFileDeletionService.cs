using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Service.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for deleting temporary files and browser cache folders on Windows.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Bereinigung von Dateisystem-Inhalten.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortFileDeletion"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um physische Dateilöschungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsFileDeletionService : IOutboundPortFileDeletion
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string AccessDeniedDirectoryLogMessage = "Access denied to directory: {Dir}";
    private const string AllFilesSearchPattern = "*";
    private const int DefaultReportInterval = 10;
    private const string DeletingFileStatusFormat = "Deleting: {0}";
    private const string DirectoryCouldNotBeDeletedLogMessage = "Directory could not be deleted (it might not be empty): {Dir}";
    private const string ErrorDeletingFileLogMessage = "Error while deleting individual file: {FilePath}";
    private const string ErrorDeletingFilePayloadLogMessage = "Error while deleting file payload: {File}";
    private const string ErrorGatheringFilesLogMessage = "Error while gathering files from directory: {Dir}";
    private const string FinalizingCleanupStatusMessage = "Finalizing cleanup operations...";
    private const int InitialCounterValue = 0;
    private const string ProtectedFirefoxExtensionFilter = "moz-extension";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly ILogger<AdapterWindowsFileDeletionService> _logger;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AdapterWindowsFileDeletionService(ILogger<AdapterWindowsFileDeletionService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    public Task<int> CountFilesAsync(List<string> directories)
    {
        // ⚡ Immediate Guard-Clause Exception Timing & Direct Task Return (Guide Abs. 9.1 & 15)
        ArgumentNullException.ThrowIfNull(directories);

        return Task.Run(() =>
        {
            return directories.Where(Directory.Exists).Sum(dir =>
            {
                try
                {
                    // ✅ .NET 10: EnumerateFiles streamt Pfade allokationsarm ohne temporäre string[]-Arrays auf dem Heap
                    return Directory.EnumerateFiles(dir, AllFilesSearchPattern, SearchOption.AllDirectories).Count();
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(exception, AccessDeniedDirectoryLogMessage, dir);
                    }

                    return InitialCounterValue;
                }
            });
        });
    }

    public Task DeleteFilesAsync(List<string> directories, IProgress<string> statusReporter, IProgress<int> valueReporter, CancellationToken token)
    {
        // ⚡ Immediate Guard-Clause Exception Timing & Direct Task Return (Guide Abs. 9.1 & 15)
        ArgumentNullException.ThrowIfNull(directories);
        ArgumentNullException.ThrowIfNull(statusReporter);
        ArgumentNullException.ThrowIfNull(valueReporter);

        return Task.Run(() =>
        {
            var context = new DeletionContext
            {
                DeletedCount = InitialCounterValue,
                UpdateCounter = InitialCounterValue,
                ReportInterval = DefaultReportInterval,
                StatusReporter = statusReporter,
                ValueReporter = valueReporter,
                Token = token
            };

            foreach (var dir in directories)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                ProcessDirectory(dir, context, _logger);
            }

            // AT THE END: Dispatch a final progress update to report completion status in case items were skipped due to the reporting threshold interval
            statusReporter.Report(FinalizingCleanupStatusMessage);
            valueReporter.Report(context.DeletedCount);

        }, token);
    }

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
                _logger.LogWarning(exception, ErrorDeletingFileLogMessage, filePath);
            }
        }
    }

    private static void ProcessDirectory(string dir, DeletionContext context, ILogger logger)
    {
        if (!Directory.Exists(dir))
        {
            return;
        }

        IEnumerable<string> files;

        try
        {
            // ✅ .NET 10: EnumerateFiles vermeidet Allokation riesiger string[]-Arrays im RAM bei großen Caches
            files = Directory.EnumerateFiles(dir, AllFilesSearchPattern, SearchOption.AllDirectories);
        }
        catch (Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(exception, ErrorGatheringFilesLogMessage, dir);
            }

            return;
        }

        foreach (var file in files)
        {
            if (context.Token.IsCancellationRequested)
            {
                return;
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
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(exception, DirectoryCouldNotBeDeletedLogMessage, dir);
            }
        }
    }

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

            // Only dispatch status updates when the defined 'ReportInterval' window has passed
            if (context.UpdateCounter >= context.ReportInterval)
            {
                string statusMessage = string.Format(DeletingFileStatusFormat, Path.GetFileName(file));
                context.StatusReporter.Report(statusMessage);
                context.ValueReporter.Report(context.DeletedCount);
                context.UpdateCounter = InitialCounterValue; // Reset
            }
        }
        catch (Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(exception, ErrorDeletingFilePayloadLogMessage, file);
            }
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
        public IProgress<int> ValueReporter { get; set; } = null!;
        public CancellationToken Token { get; set; }
    }
}