using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces;

public interface IBrowserExtensionDeploymentService
{
    /// <summary>
    /// Stellt sicher, dass die Chrome-Erweiterung im korrekten Zielverzeichnis (z. B. AppData) liegt.
    /// Kopiert die Dateien, falls sie dort noch nicht existieren.
    /// </summary>
    void EnsureExtensionIsDeployed();

    /// <summary>
    /// Gibt den aktuellen Pfad zum Ordner der Erweiterung zurück (Debug vs. Release).
    /// </summary>
    /// <returns>Der absolute Pfad zum Extension-Ordner.</returns>
    string GetExtensionFolderPath();
}
