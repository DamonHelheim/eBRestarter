namespace eBRestarter.Classes.EBInterfaces
{
    public interface IRegexPattern
    {
        public const string BrowserSelection                = "(Firefox|Chrome|Edge|Brave)";
        public const string BrowserSelection_Hyphen         = "(-|Firefox|Chrome|Edge|Brave)";
        public const string DeleteCookiesAndCacheSelection  = "(false|1|3|7|14)";
        public const string RestartComputerSelection        = "(false|1|3|7|14)";
        public const string ThemeSelection                  = "(Dark|Light)";
    }
}
