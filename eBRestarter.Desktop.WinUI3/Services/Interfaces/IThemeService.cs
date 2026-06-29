namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IThemeService
{
    string CurrentTheme { get; }

    void SetTheme(string themeName);
}
