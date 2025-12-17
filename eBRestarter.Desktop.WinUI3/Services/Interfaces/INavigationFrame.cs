using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{
    /// <summary>
    /// Definiert eine Abstraktionsschicht für den Navigations-Frame der Anwendung.
    /// <br/>
    /// <b>Zweck:</b> Dieses Interface entkoppelt die Navigationslogik von der konkreten WinUI-<see cref="Frame"/>-Klasse.
    /// Dies ist essenziell für Unit-Tests, da so der Frame durch ein Mock-Objekt ersetzt werden kann.
    /// </summary>
    public interface INavigationFrame
    {
        /// <summary>
        /// Navigiert zu der angegebenen Seite.
        /// </summary>
        /// <param name="sourcePageType">Der Typ der Seite (Page), zu der navigiert werden soll.</param>
        /// <param name="parameter">Ein optionales Parameter-Objekt, das an die Zielseite übergeben wird.</param>
        /// <param name="infoOverride">Informationen zur Navigationsanimation (z.B. DrillIn, Slide).</param>
        /// <returns><c>true</c>, wenn die Navigation erfolgreich initiiert wurde; andernfalls <c>false</c>.</returns>
        bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride);

        /// <summary>
        /// Gibt an, ob der Navigationsverlauf Einträge enthält, zu denen zurückgekehrt werden kann.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Navigiert zur vorherigen Seite im Navigationsverlauf zurück.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Ruft den aktuellen Inhalt (die aktuell angezeigte Seite) des Frames ab.
        /// Wird verwendet, um doppelte Navigationen zur gleichen Seite zu verhindern.
        /// </summary>
        object Content { get; }
    }
}
