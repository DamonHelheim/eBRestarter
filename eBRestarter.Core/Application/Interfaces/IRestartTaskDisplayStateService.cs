using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Domain.Entities;

namespace eBRestarter.Core.Application.Interfaces;

/// <summary>
/// Port: Erzeugt den initialen Anzeige-Zustand für die Restart-Task-UI aus Config und Lokalisierung.
/// </summary>
public interface IRestartTaskDisplayStateService
{

    /// <summary>
    /// Baut das Anzeige-DTO aus der aktuellen Config (inkl. Cache-Lösch-Status und formatierte Texte).
    /// </summary>
    RestartTaskDisplayState GetInitialState(AppConfig config);
}
