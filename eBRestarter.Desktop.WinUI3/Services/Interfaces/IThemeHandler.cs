namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IThemeHandler
{
    string CurrentTheme { get; }

    void SetTheme(string themeName);
}
