namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface ILanguageHandler
{
    string CurrentLanguageCode { get; }

    void SetLanguage(string languageCode);
}
