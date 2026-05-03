namespace eBRestarter.Desktop.WinUI3.Models;

public sealed record IconCredit
{
    public string IconCreator { get; set; } = string.Empty;
    public string IconImageSource { get; init; } = string.Empty;
    public string IconHyperLink { get; init; } = string.Empty;
    public string IconHyperLinkContent { get; init; } = string.Empty;
}
