using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{
    public interface IThemeService
    {
        // "Light" or "Dark"
        void SetTheme(string themeName);
        string CurrentTheme { get; }
    }
}
