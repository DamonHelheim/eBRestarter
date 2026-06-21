using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services;

public class FileDeletionHandler : IFileDeletionUseCase
{
    public async Task<int> CountFilesAsync(List<string> directories)
    {
        return await Task.Run(() =>
        {
            int count = 0;
            foreach (var dir in directories)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        count += Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
                    }
                    catch { /* Zugriff ignorieren */ }
                }
            }
            return count;
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
                ProcessDirectory(dir, context);
            }

            // AM ENDE: Einmal final 100% / Fertig melden, falls durch das Intervall was fehlte
            statusReporter.Report("AbschlieÃŸe Bereinigung...");
            valueReporter.Report(context.DeletedCount);

        }, token);
    }

    public void DeleteSingleFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try { File.Delete(filePath); } catch { }
        }
    }

    private static void ProcessDirectory(string dir, DeletionContext context)
    {
        if (!Directory.Exists(dir)) return;

        string[] files;

        try { files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories); }
        catch { return; }

        foreach (var file in files)
        {
            if (context.Token.IsCancellationRequested) return;
            ProcessFile(file, context);
        }

        // Ordner lÃ¶schen
        if (context.Token.IsCancellationRequested) return;

        // So wird der Ordner NUR gelÃ¶scht, wenn er leer ist.
        // Wenn eine "moz-extension" Datei Ã¼brig blieb, bleibt auch der Ordner bestehen.
        try { Directory.Delete(dir, false); } catch { }
    }

    private static void ProcessFile(string file, DeletionContext context)
    {
        try
        {
            if (file.Contains("moz-extension")) return;

            File.Delete(file);
            context.DeletedCount++;
            context.UpdateCounter++;

            // Wir senden den Status nur, wenn 'reportInterval' erreicht ist
            if (context.UpdateCounter >= context.ReportInterval)
            {
                context.StatusReporter.Report($"LÃ¶sche: {Path.GetFileName(file)}");
                context.ValueReporter.Report(context.DeletedCount);
                context.UpdateCounter = 0; // Reset
            }
        }
        catch { /* Ignorieren */ }
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


