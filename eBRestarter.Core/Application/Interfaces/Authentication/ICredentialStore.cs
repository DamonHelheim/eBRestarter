using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Authentication;

public interface ICredentialStore
{
    // Speichert Username & Key sicher
    void SaveCredentials(ApiCredentials credentials);

    // Lädt die gespeicherten Daten (falls vorhanden)
    ApiCredentials? LoadCredentials();

    // Löscht die Daten (Reset)
    void ClearCredentials();

    // Importiert aus einer alten Datei (für Migration)
    ApiCredentials? ImportFromLegacyFile(string filePath);
}
