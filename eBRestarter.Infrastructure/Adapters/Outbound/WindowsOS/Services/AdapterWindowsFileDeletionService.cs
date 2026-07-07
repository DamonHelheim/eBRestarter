using eBRestarter.Core.Application.Ports.Outbound.Interfaces;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for deleting temporary files and browser cache folders on Windows.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Bereinigung von Dateisystem-Inhalten.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortFileDeletion"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um physische Dateilöschungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsFileDeletionService(ILogger<AdapterWindowsFileDeletionService> logger) : IOutboundPortFileDeletion
{
    private readonly ILogger<AdapterWindowsFileDeletionService> _logger = logger;

    public async Task<int> CountFilesAsync(List<string> directories)
    {
        return await Task.Run(() =>
        {
            return directories.Where(Directory.Exists).Sum(dir =>
            {
                try
                {
                    return Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(ex, "Access denied to directory: {Dir}", dir);
                    }
                    return 0;
                }
            });
        });
    }

    public async Task DeleteFilesAsync(List<string> directories, IProgress<string> statusReporter, IProgress<int> valueReporter, CancellationToken token)
    {
        await Task.Run(() =>
        {
            var context = new DeletionContext
            {
                DeletedCount = 0,
                UpdateCounter = 0,
                ReportInterval = 10,
                StatusReporter = statusReporter,
                ValueReporter = valueReporter,
                Token = token
            };

            foreach (var dir in directories)
            {
                if (token.IsCancellationRequested) return;
                ProcessDirectory(dir, context, _logger);
            }

            // AT THE END: Dispatch a final progress update to report completion status in case items were skipped due to the reporting threshold interval
            statusReporter.Report("Finalizing cleanup operations...");
            valueReporter.Report(context.DeletedCount);

        }, token);
    }

    public void DeleteSingleFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while deleting individual file: {FilePath}", filePath);
            }
        }
    }

    private static void ProcessDirectory(string dir, DeletionContext context, ILogger logger)
    {
        if (!Directory.Exists(dir)) return;

        string[] files;

        try
        {
            files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(ex, "Error while gathering files from directory: {Dir}", dir);
            }
            return;
        }

        foreach (var file in files)
        {
            if (context.Token.IsCancellationRequested) return;
            ProcessFile(file, context, logger);
        }

        // Delete directory node
        if (context.Token.IsCancellationRequested) return;

        // The parameter setup evaluates recursive: false. Thus, the folder is ONLY deleted if it is completely empty.
        // If system configurations or specialized files like "moz-extension" remain, the parent directory container will persist.
        try
        {
            Directory.Delete(dir, recursive: false);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(ex, "Directory could not be deleted (it might not be empty): {Dir}", dir);
            }
        }
    }

    private static void ProcessFile(string file, DeletionContext context, ILogger logger)
    {
        try
        {
            // Do not delete protected engine assets like specific Firefox components
            if (file.Contains("moz-extension")) return;

            File.Delete(file);
            context.DeletedCount++;
            context.UpdateCounter++;

            // Only dispatch status updates when the defined 'ReportInterval' window has passed
            if (context.UpdateCounter >= context.ReportInterval)
            {
                context.StatusReporter.Report($"Deleting: {Path.GetFileName(file)}");
                context.ValueReporter.Report(context.DeletedCount);
                context.UpdateCounter = 0; // Reset
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(ex, "Error while deleting file payload: {File}", file);
            }
        }
    }

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