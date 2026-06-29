namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface ILanguageService
{
    string CurrentLanguageCode { get; }

    void SetLanguageOption(string languageCode);
}
