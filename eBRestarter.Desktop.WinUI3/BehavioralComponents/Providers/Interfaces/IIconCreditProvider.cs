using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.PresentationDTO;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

public interface IIconCreditProvider
{
    List<IconCredit> GetIconCredits();
}
