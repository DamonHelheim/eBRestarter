using eBRestarter.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Browser
{
    //(Wie erstelle ich einen?)
    public interface IBrowserFactory
    {
        IBrowser Create(BrowserType type);
    }
}
