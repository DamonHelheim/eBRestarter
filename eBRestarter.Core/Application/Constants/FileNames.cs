namespace eBRestarter.Core.Application.Constants;

public static class FileNames
{
    // Dateinamen / Keys
    public const string EbRestarterConfig = "eBRestarterConfig";
    public const string DeleteDate = "DeleteDate";
    public const string RestartDate = "RestartDate";

    // Browser Extensions
    // Hinweis: Führende Backslashes entfernt -> Nutze Path.Combine() im Code!
    public const string EbesucherAddOnNameForFirefox = "{fef425dc-a60f-4484-954d-71ecf2544846}.xpi";

    // Chrome & Edge nutzen oft die gleiche ID, wenn sie aus dem Chrome Store kommen.
    // Prüfe bitte, ob Edge wirklich eine eigene ID hat (kjhej...) oder die Chrome-ID nutzt.
    public const string EbesucherAddOnNameForChrome = "agchmcconfdfcenopioeilpgjngelefk";
    public const string EbesucherAddOnNameForEdge = "kjhejmaladginnedpoppohfnkionnghi";
}
