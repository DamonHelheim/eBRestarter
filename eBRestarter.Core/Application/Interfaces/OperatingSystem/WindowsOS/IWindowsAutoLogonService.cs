using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    public interface IWindowsAutoLogonService
    {
        /// <summary>
        /// Aktiviert die automatische Windows-Anmeldung.
        /// </summary>
        /// <param name="username">Der Benutzername.</param>
        /// <param name="domain">Die Domäne (oder leer/Rechnername für lokal).</param>
        /// <param name="password">Das Klartext-Passwort.</param>
        void EnableAutoLogon(string username, string domain, string password);

        /// <summary>
        /// Deaktiviert die automatische Anmeldung und löscht das gespeicherte Passwort.
        /// </summary>
        void DisableAutoLogon();

        /// <summary>
        /// Prüft, ob AutoLogon aktuell aktiv konfiguriert ist.
        /// </summary>
        bool IsAutoLogonEnabled();
    }
}
