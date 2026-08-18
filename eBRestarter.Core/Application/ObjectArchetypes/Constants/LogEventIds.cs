namespace eBRestarter.Core.Application.ObjectArchetypes.Constants;

/// <summary>
/// Central registry of all log <c>EventId</c> values used across the solution.
/// </summary>
/// <remarks>
/// <para>
/// <b>Architectural Rationale:</b> Event ID constants are located in the Core application layer using primitive integer values
/// without framework dependencies, maintaining hexagonal isolation while serving as a single source of truth across all layers.
/// </para>
/// <para>
/// <b>ID Assignment Rule:</b> New event IDs must be appended to the end of their respective functional range. Existing IDs must not
/// be reassigned or reordered to maintain telemetry and alert query compatibility.
/// </para>
/// <para>
/// Comprehensive logging conventions and reference implementations are documented in <c>LOGGING.md</c>.
/// </para>
/// </remarks>
public static class LogEventIds
{

    /// <summary>Event IDs 1000–1999: Application configuration loading, saving, and encryption.</summary>
    public static class Configuration
    {
        public const int ConfigurationSaved = 1000;
        public const int ConfigurationSaveFailed = 1001;
        public const int ConfigurationInvalidJson = 1002;
        public const int ConfigurationLoadFailed = 1003;
        public const int ConfigurationReset = 1004;
        public const int ApiKeyDecryptionFailed = 1010;
        public const int ApiKeyEncryptionFailed = 1011;
        public const int CredentialStoreLoadFailed = 1020;
        public const int CredentialStoreLegacyImportFailed = 1021;
    }

    /// <summary>Event IDs 2000–2999: Browser discovery, process lifecycle, profile, and extension handling.</summary>
    public static class Browser
    {
        public const int BrowserStarting = 2000;
        public const int BrowserStartFailed = 2001;
        public const int BrowserTerminationFailed = 2002;
        public const int BrowserProfileDiscoveryFailed = 2010;
        public const int BrowserTypeNotSupported = 2011;
        public const int BrowserDetailEvaluationFailed = 2012;
        public const int BrowserProfileFileUnreadable = 2013;
        public const int BrowserCacheDirectoryUnreadable = 2020;
        public const int BrowserCacheFileDeletionFailed = 2021;
        public const int BrowserCacheDirectoryNotRemoved = 2022;
        public const int BrowserCacheReparsePointSkipped = 2023;
        public const int BrowserCachePathOutsideRoot = 2024;
    }

    /// <summary>Event IDs 3000–3999: Restarter cycle execution, scheduled system reboots, and timing.</summary>
    public static class RestarterCycle
    {
        public const int RestartSchedulerStarted = 3000;
        public const int RestartSchedulerStopped = 3001;
        public const int RestartSchedulerLoopFaulted = 3002;
        public const int RestartSlotMissed = 3003;
        public const int RestartSequenceInitiated = 3010;
        public const int RestartClosingApplications = 3011;
        public const int RestartDispatchingReboot = 3012;
        public const int RestartSequenceFailed = 3013;
        public const int CycleFaulted = 3020;
        public const int FireAndForgetTaskFaulted = 3021;
    }

    /// <summary>Event IDs 4000–4999: Application updates and file downloads.</summary>
    public static class Update
    {
        public const int UpdateCheckFailed = 4000;
        public const int UpdateResponseParsingFailed = 4001;
        public const int UpdateDownloadUrlMissing = 4002;
        public const int UpdateDownloaded = 4003;
        public const int BrowserInstallerCleanupFailed = 4010;
    }

    /// <summary>Event IDs 5000–5999: Operating system integration (registry, process control, WMI, autostart).</summary>
    public static class OperatingSystem
    {
        public const int ProcessStarted = 5000;
        public const int ProcessStartFailed = 5001;
        public const int ProcessTerminationFailed = 5002;
        public const int ProcessCloseRequestFailed = 5003;
        public const int ProcessShutdownRequested = 5004;
        public const int ProcessShutdownFailed = 5005;
        public const int ProcessTerminated = 5006;
        public const int ProcessesClosedGracefully = 5007;
        public const int ProcessCloseTimeout = 5008;
        public const int ExplorerOpened = 5010;
        public const int ExplorerOpenFailed = 5011;
        public const int InstallerOutputReceived = 5020;
        public const int InstallerStartFailed = 5021;
        public const int InstallerNotStarted = 5022;
        public const int AutoStartEnabled = 5030;
        public const int AutoStartDisabled = 5031;
        public const int AutoStartOperationFailed = 5032;
        public const int AutoStartStatusReadFailed = 5033;
        public const int WmiQueryFailed = 5040;
        public const int OsVersionReadFailed = 5041;
        public const int RegistryReadFailed = 5042;
        public const int DefaultBrowserMismatch = 5043;
        public const int EdgeStartupBoostChanged = 5050;
        public const int EdgeStartupBoostReadFailed = 5051;
    }

    /// <summary>Event IDs 6000–6999: External API requests and network interactions.</summary>
    public static class Api
    {
        public const int ApiRequestFailed = 6000;
        public const int ApiRequestTimedOut = 6001;
        public const int ApiErrorResponse = 6002;
        public const int ApiResponseParsingFailed = 6010;
        public const int EarningsRetrievalFailed = 6011;
    }

    /// <summary>Event IDs 7000–7999: Presentation layer, view models, dialogs, and localization.</summary>
    public static class UserInterface
    {
        public const int StartupConfigurationFailed = 7000;
        public const int ThemeLoadFailed = 7001;
        public const int SchedulerStartFailed = 7002;
        public const int HostDisposeFailed = 7003;
        public const int UnhandledUiException = 7004;
        public const int LocalizationLookupFailed = 7010;
        public const int ViewModelOperationFailed = 7020;
        public const int ExtensionConfigReadFailed = 7030;
        public const int ExtensionConfigWriteFailed = 7031;
    }

    /// <summary>Event IDs 9000–9999: Security-critical events and auditing.</summary>
    public static class Security
    {
        public const int AutoLogonEnabled = 9000;
        public const int AutoLogonDisabled = 9001;
        public const int AutoLogonConfigurationFailed = 9002;
        public const int PasswordlessModeChanged = 9010;
        public const int PasswordlessRollbackFailed = 9011;
        public const int PasswordlessRegistryKeyMissing = 9012;
        public const int PasswordlessReadFailed = 9013;
        public const int CredentialValidationRejected = 9020;
        public const int CredentialValidationErrored = 9021;
        public const int SignatureVerificationRejected = 9030;
        public const int SignatureVerificationErrored = 9031;
        public const int DownloadHostRejected = 9040;
        public const int DownloadSchemeRejected = 9041;
        public const int UnsafeUrlLaunchBlocked = 9042;
    }
}
