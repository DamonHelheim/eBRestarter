namespace eBRestarter.Classes.Cache
{
    public sealed class SettingsCache
    {
        public static IDictionary<string, string> Position { get; set; } = new Dictionary<string, string>();
        public static IDictionary<string, string> APIPosition { get; set; } = new Dictionary<string, string>();

        private static readonly SettingsCache instance = new();
        public static SettingsCache Instance { get { return instance; } }
        static SettingsCache() { }
        private SettingsCache() { }

    }
}
