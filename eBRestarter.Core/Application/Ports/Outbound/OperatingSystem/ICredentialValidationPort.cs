namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

/// <summary>
/// Port für die Validierung von Windows-/Domain-Anmeldedaten (z.B. Auto-Logon).
/// Die Implementierung kann bei Domain-Fehlern eine Exception werfen.
/// </summary>
public interface ICredentialValidationPort
{
    /// <summary>
    /// Validiert Benutzername/Domain/Passwort gegen den lokalen Rechner oder die Domain.
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <param name="domain">Domain oder Rechnername</param>
    /// <param name="password">Passwort</param>
    /// <returns>true wenn gültig, sonst false. Kann bei Domain-Fehlern Exception werfen.</returns>
    bool ValidateCredentials(string username, string domain, string password);
}



