using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.Models;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IIconCreditHandler
{
    List<IconCredit> GetIconCredits();
}
