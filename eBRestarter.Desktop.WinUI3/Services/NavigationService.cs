using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services
{
    /// <summary>
    /// Der zentrale Navigationsdienst der Anwendung.
    /// Verwaltet die Registrierung von Routen und führt die Navigation über einen abstrahierten Frame-Adapter aus.
    /// </summary>
    public sealed class NavigationService : INavigationService
    {
        /// <summary>
        /// Speichert die Zuordnung von Navigations-Schlüsseln (Strings) zu den Seitentypen (Types).
        /// </summary>
        private readonly Dictionary<string, Type> _pages = [];

        /// <summary>
        /// Die Abstraktion des Frames. Zur Laufzeit ist dies der <see cref="WinUIFrameAdapter"/>,
        /// in Tests ist dies ein Mock-Objekt.
        /// </summary>
        private INavigationFrame? _frameAdapter;

        /// <summary>
        /// Verbindet den Service mit dem physischen Frame der Anwendung (View).
        /// Sollte im Konstruktor des Hauptfensters (MainWindow) aufgerufen werden.
        /// </summary>
        /// <param name="frame">Der XAML-Frame, in dem die Navigation stattfinden soll.</param>
        public void AttachFrame(Frame frame)
        {
            // Hier kapseln wir den echten Frame in unseren Adapter
            _frameAdapter = new WinUIFrameAdapter(frame);
        }

        /// <summary>
        /// Ermöglicht das Injizieren eines Mock-Frames für Unit-Tests.
        /// <br/>
        /// <b>Hinweis:</b> Diese Methode ist <c>internal</c> und sollte nur vom Testprojekt aus aufgerufen werden
        /// (via [InternalsVisibleTo]).
        /// </summary>
        /// <param name="frameAdapter">Das Mock-Objekt, das <see cref="INavigationFrame"/> implementiert.</param>
        internal void SetFrameAdapter(INavigationFrame frameAdapter)
        {
            _frameAdapter = frameAdapter;
        }

        /// <summary>
        /// Navigiert zu einer registrierten Seite basierend auf ihrem Schlüssel (Key).
        /// </summary>
        /// <param name="key">Der eindeutige Schlüssel der Zielseite (z.B. "Settings").</param>
        /// <param name="parameter">Ein optionales Parameter-Objekt für das ViewModel der Zielseite.</param>
        /// <param name="transitionInfo">Optionale Übergangsanimation.</param>
        /// <returns>
        /// <c>true</c>, wenn die Navigation erfolgreich war. 
        /// <c>false</c>, wenn der Key unbekannt ist, kein Frame verbunden ist oder die Seite bereits aktiv ist.
        /// </returns>
        public bool NavigateTo(string key, object parameter = null, NavigationTransitionInfo transitionInfo = null)
        {
            // 1. Validierung: Ist ein Frame vorhanden?
            if (_frameAdapter == null) return false;

            // 2. Suche: Existiert der Key in der Routen-Tabelle?
            if (_pages.TryGetValue(key, out var pageType))
            {
                // 3. Prüfung: Sind wir bereits auf dieser Seite? (Verhindert unnötiges Neuladen)
                // Wir prüfen hier gegen den Adapter, nicht gegen UI-Elemente direkt.
                if (_frameAdapter.Content?.GetType() == pageType) return false;

                // 4. Ausführung: Navigation über den Adapter anstoßen
                return _frameAdapter.Navigate(pageType, parameter, transitionInfo);
            }

            // Route wurde nicht gefunden
            return false;
        }

        /// <summary>
        /// Registriert eine neue Route im Navigationsdienst.
        /// </summary>
        /// <param name="key">Der eindeutige Identifikator für die Route (z.B. "Home").</param>
        /// <param name="pageType">Der Typ der Page-Klasse (typeof(MyPage)).</param>
        public void RegisterRoute(string key, Type pageType)
        {
            if (!_pages.ContainsKey(key))
            {
                _pages.Add(key, pageType);
            }
        }
    }
}





























//public bool Navigate<TPage>(object? parameter = null, NavigationTransitionInfo? infoOverride = null) where TPage : Page
//{
//    infoOverride = new DrillInNavigationTransitionInfo();

//    if (_frame is null) return false;

//    if (infoOverride is null)
//    {
//        // Standard-Navigation ohne explizite Transition
//        return _frame.Navigate(typeof(TPage), parameter);
//    }

//    return _frame.Navigate(typeof(TPage), parameter, infoOverride);
//}

//public bool Navigate( Type pageType, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
//{
//    infoOverride = new DrillInNavigationTransitionInfo();

//    if (_frame is null) return false;

//    if (infoOverride is null)
//    {
//        return _frame.Navigate(pageType, parameter);
//    }

//    return _frame.Navigate(pageType, parameter, infoOverride);
//}
