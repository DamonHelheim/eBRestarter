using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ResourceLoader _resourceLoader;

        public LocalizationService()
        {
            // Hier ist die Windows-spezifische Instanziierung erlaubt und gekapselt
            _resourceLoader = new ResourceLoader();
        }

        public string GetString(string key)
        {
            var result = _resourceLoader.GetString(key);
            return string.IsNullOrEmpty(result) ? $"[{key}]" : result;
        }

        public IEnumerable<LanguageOption> GetAvailableLanguages()
        {
            // Nutzt die interne Methode, um die Texte zu holen
            string de = GetString("Lang_German"); // Falls Key leer -> Fallback "Deutsch"
            string en = GetString("Lang_English");

            if (de.StartsWith("[")) de = "Deutsch";
            if (en.StartsWith("[")) en = "English";

            return
            [
                new(de, 0),
                new(en, 1)
            ];
        }

        public IEnumerable<ComputerRestartOption> GetComputerRestartOptions()
        {
            return
    [
        // Wir nutzen die Hilfsmethode GetString, die du schon hast (mit Fallback)
        new(GetString("RestartOption_0"), 0),
        new(GetString("RestartOption_1"), 1),
        new(GetString("RestartOption_3"), 3),
        new(GetString("RestartOption_7"), 7),
        new(GetString("RestartOption_14"), 14)
    ];
        }


        public IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions()
        {
            return
    [
        // Texte dynamisch laden
        new(GetString("BrowserCacheOption_0"), 0),
        new(GetString("BrowserCacheOption_1"), 1),
        new(GetString("BrowserCacheOption_3"), 3),
        new(GetString("BrowserCacheOption_7"), 7),
        new(GetString("BrowserCacheOption_14"), 14)
    ];
        }
    }
}
