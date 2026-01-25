using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Windows.Globalization;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    public class LanguageService : ILanguageService
    {
        public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

        public void SetLanguage(string languageCode)
        {
            // Diese Zeile sorgt dafür, dass beim nächsten Laden von Ressourcen 
            // (z.B. beim Neustart oder Laden einer neuen Page) die neue Sprache gewählt wird.
            ApplicationLanguages.PrimaryLanguageOverride = languageCode;
        }
    }
}
