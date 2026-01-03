using eBRestarter.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.Services
{
    public class FileDeletionService : IFileDeletionService
    {
        // Wir nutzen System.IO direkt oder deinen FileSystemService Wrapper, falls vorhanden.
        // Hier System.IO der Einfachheit halber.

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
                        catch { /* Zugriff verweigert ignorieren */ }
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

                foreach (var dir in directories)
                {
                    if (!Directory.Exists(dir)) continue;

                    // 1. Alle Dateien holen

                    string[] files;

                    try { files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories); }

                    catch { continue; }

                    foreach (var file in files)
                    {
                        if (token.IsCancellationRequested) return;

                        try
                        {
                            // Schutz vor Addons (aus deinem alten Code)
                            if (file.Contains("moz-extension")) continue;

                            File.Delete(file);
                            deletedCount++;

                            statusReporter.Report($"Gelöscht: {Path.GetFileName(file)}");
                            valueReporter.Report(deletedCount);
                        }
                        catch
                        {
                            // Ignorieren (in Benutzung etc.)
                        }
                    }

                    // 2. Leere Ordner aufräumen (Optional)
                    // try { Directory.Delete(dir, true); } catch {} 
                }
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
}
