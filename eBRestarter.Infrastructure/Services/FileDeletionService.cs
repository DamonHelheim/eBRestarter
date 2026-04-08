using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services;

public class FileDeletionService : IFileDeletionService
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
            int deletedCount = 0;
            // Variable für das Drosseln der Updates
            const int reportInterval = 10; // Nur alle 50 Dateien die UI updaten
            int updateCounter = 0;

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir)) continue;

                string[] files;

                try { files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories); }

                catch { continue; }

                foreach (var file in files)
                {
                    if (token.IsCancellationRequested) return;

                    try
                    {
                        if (file.Contains("moz-extension")) continue;

                        File.Delete(file);
                        deletedCount++;
                        updateCounter++;

                        // --- PERFORMANCE FIX: Drosselung ---
                        // Wir senden den Status nur, wenn 'reportInterval' erreicht ist
                        // ODER wenn es die allerletzte Datei ist (damit 100% sicher erreicht wird).
                        if (updateCounter >= reportInterval)
                        {
                            statusReporter.Report($"Lösche: {Path.GetFileName(file)}");
                            valueReporter.Report(deletedCount);
                            updateCounter = 0; // Reset
                        }
                    }
                    catch { /* Ignorieren */ }
                }

                // Ordner löschen
                if (token.IsCancellationRequested) return;

                // So wird der Ordner NUR gelöscht, wenn er leer ist.
                // Wenn eine "moz-extension" Datei übrig blieb, bleibt auch der Ordner bestehen.
                try { Directory.Delete(dir, false); } catch { }
            }

            // AM ENDE: Einmal final 100% / Fertig melden, falls durch das Intervall was fehlte
            statusReporter.Report("Abschließe Bereinigung...");
            valueReporter.Report(deletedCount);

        }, token);
    }

    public void DeleteSingleFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try { File.Delete(filePath); } catch { }
        }
    }
}

