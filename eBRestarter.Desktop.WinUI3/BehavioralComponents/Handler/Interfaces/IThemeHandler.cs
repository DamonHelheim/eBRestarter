namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;

public interface IThemeHandler
{
    string CurrentTheme { get; }

    void SetTheme(string themeName);
}
