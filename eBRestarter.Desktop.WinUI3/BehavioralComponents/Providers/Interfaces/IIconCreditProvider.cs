using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.PresentationDTO;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

/// <summary>
/// Provider interface for retrieving UI icon credits and attribution information.
/// </summary>
public interface IIconCreditProvider
{
    /// <summary>
    /// Retrieves a list of icon attribution objects containing creator, source path, and web links.
    /// </summary>
    /// <returns>A list of <see cref="IconCredit"/> items.</returns>
    List<IconCredit> GetIconCredits();
}
