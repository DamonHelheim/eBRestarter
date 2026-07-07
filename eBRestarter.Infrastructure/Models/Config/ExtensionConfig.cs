using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.Models.Config;

public sealed record ExtensionConfig
{
    public string LANGUAGE { get; init; } = "DE";
    public string ZIEL_URL { get; init; } = WebLinks.EVisitorSurflink;
    public int WARTEZEIT_MS { get; init; } = 180000;
}


