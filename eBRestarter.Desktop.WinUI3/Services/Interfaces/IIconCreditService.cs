using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.Models;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IIconCreditService
{
    List<IconCredit> GetIconCredits();
}
