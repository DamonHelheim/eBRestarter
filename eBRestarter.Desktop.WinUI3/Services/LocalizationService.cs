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
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing state)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly ResourceLoader _resourceLoader;

        #endregion

        // =========================================================
        // 2. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public LocalizationService()
        {
            _resourceLoader = new ResourceLoader();
        }

        #endregion

        // =========================================================
        // 3. PUBLIC METHODS
        // =========================================================
        #region PublicMethods

        public string GetString(string key)
        {
            var result = _resourceLoader.GetString(key);
            return string.IsNullOrEmpty(result) ? $"[{key}]" : result;
        }

        public IEnumerable<LanguageOption> GetAvailableLanguages()
        {
            string de = GetString("Lang_German");
            string en = GetString("Lang_English");
            if (de.StartsWith("[")) de = "Deutsch";
            if (en.StartsWith("[")) en = "English";
            return [new(de, 0), new(en, 1)];
        }

        public IEnumerable<ComputerRestartOption> GetComputerRestartOptions()
        {
            return
            [
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
                new(GetString("BrowserCacheOption_0"), 0),
                new(GetString("BrowserCacheOption_1"), 1),
                new(GetString("BrowserCacheOption_3"), 3),
                new(GetString("BrowserCacheOption_7"), 7),
                new(GetString("BrowserCacheOption_14"), 14)
            ];
        }

        #endregion
    }
}
