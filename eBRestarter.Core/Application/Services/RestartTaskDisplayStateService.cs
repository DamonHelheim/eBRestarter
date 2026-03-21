using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Domain.Models.Records.Config;

namespace eBRestarter.Core.Application.Services;

/// <summary>
/// Erzeugt RestartTaskDisplayState aus Config und Lokalisierung (Move aus ViewModelRestartTask LoadInitialData).
/// </summary>
public class RestartTaskDisplayStateService(
    ILocalizationService localizationService,
    ICacheDeletionIntervalValidator intervalValidator,
    TimeProvider timeProvider) : IRestartTaskDisplayStateService
{
    // =========================================================
    // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
    // =========================================================
    #region FieldsAndInjectedServices

    private readonly ILocalizationService _localizationService = localizationService;
    private readonly ICacheDeletionIntervalValidator _intervalValidator = intervalValidator;
    private readonly TimeProvider _timeProvider = timeProvider;

    #endregion
    #region ConstructorAndFinalizer

    #endregion

    // =========================================================
    // 3. PUBLIC & PROTECTED METHODS (API)
    // =========================================================
    #region PublicAndProtectedMethods

    /// <inheritdoc />
    public RestartTaskDisplayState GetInitialState(AppConfig config)
    {
        if (config == null)
        {
            return new RestartTaskDisplayState
            {
                ChoosenBrowser = _localizationService.GetString("Task_DefaultBrowser"),
                PauseSeconds = 20,
                RuntimeSeconds = 3600
            };
        }

        string username = config.Username ?? "-";
        string choosenBrowser = config.Browser?.Selected ?? _localizationService.GetString("Task_DefaultBrowser");

        int runtimeSeconds = config.Browser != null ? config.Browser.RuntimeHours * 3600 : 3600;
        int pauseSeconds = config.Browser?.RuntimePauseSeconds ?? 20;

        int intervalDays = config.Browser?.DeleteBrowserCacheIntervalDays ?? 0;
        DateTime nextDate = config.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;

        bool deleteBrowserContentIsActive = _intervalValidator.IsValidIntervalDays(intervalDays);

        string deleteIsActivatedMessage;
        string nextDeletionProcessMessage;
        string nextDeletionProcessDateMessage;

        if (deleteBrowserContentIsActive)
        {
            deleteIsActivatedMessage = _localizationService.GetString("Activate");
            nextDeletionProcessMessage = _localizationService.GetString("NextDeletionProcess");

            string formatPattern = _localizationService.GetString("Browser_NextDeleteDate_Format");

            var today = _timeProvider.GetLocalNow().Date;
            if (nextDate == today && intervalDays > 0)
                nextDate = today.AddDays(intervalDays);

            nextDeletionProcessDateMessage = string.Format(formatPattern, nextDate);
        }
        else
        {
            deleteIsActivatedMessage = _localizationService.GetString("Disabled");
            nextDeletionProcessMessage = string.Empty;
            nextDeletionProcessDateMessage = string.Empty;
        }

        return new RestartTaskDisplayState
        {
            Username = username,
            ChoosenBrowser = choosenBrowser,
            RuntimeSeconds = runtimeSeconds,
            PauseSeconds = pauseSeconds,
            DeleteBrowserContentIsActive = deleteBrowserContentIsActive,
            DeleteIsActivatedMessage = deleteIsActivatedMessage,
            NextDeletionProcessMessage = nextDeletionProcessMessage,
            NextDeletionProcessDateMessage = nextDeletionProcessDateMessage
        };
    }

    #endregion
}
