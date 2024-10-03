namespace eBRestarter.ViewModel.ViewModels
{
    public static class RestartBrowserTaskMediatorViewModel
    {

        public static string UsernameMediator { get; set; } = string.Empty;

        public static string ChossenBrowserMediator { get; set; } = string.Empty;

        public static string BrowserContentDeleteDate { get; set; } = string.Empty;

        public static bool? TriggerUsernameMediator { get; set; }

        public static bool? TriggerChossenBrowserMediator { get; set; }

    }
}
