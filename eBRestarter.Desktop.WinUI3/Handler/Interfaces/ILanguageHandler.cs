namespace eBRestarter.Desktop.WinUI3.Handler.Interfaces;

public interface ILanguageHandler
{
    string CurrentLanguageCode { get; }

    void SetLanguageOption(string languageCode);
}
