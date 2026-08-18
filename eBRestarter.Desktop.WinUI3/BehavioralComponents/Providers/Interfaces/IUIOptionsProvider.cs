using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

/// <summary>
/// Provider interface for retrieving dropdown options for UI selection in settings.
/// </summary>
public interface IUIOptionsProvider
{
    /// <summary>
    /// Retrieves all available language options for UI dropdown selection.
    /// </summary>
    /// <returns>A collection of <see cref="LanguageOption"/> items.</returns>
    IEnumerable<LanguageOption> GetAvailableLanguages();

    /// <summary>
    /// Retrieves computer restart interval options for UI dropdown selection.
    /// </summary>
    /// <returns>A collection of <see cref="ComputerRestartOption"/> items.</returns>
    IEnumerable<ComputerRestartOption> GetComputerRestartOptions();

    /// <summary>
    /// Retrieves browser cache deletion interval options for UI dropdown selection.
    /// </summary>
    /// <returns>A collection of <see cref="BrowserCacheDeleteOption"/> items.</returns>
    IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions();
}
