using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Application.Models.Config;
using eBRestarter.Core.Domain.Services;

namespace eBRestarter.Core.Application.Services;

/// <summary>
/// Erzeugt RestartTaskDisplayState aus Config und Lokalisierung.
/// </summary>
public class RestartTaskDisplayStateService(
    ILocalizationService localizationService,
    ICacheDeletionIntervalValidator intervalValidator,
    TimeProvider timeProvider) : IRestartTaskDisplayStateService
{
    private readonly ILocalizationService _localizationService = localizationService;
    private readonly ICacheDeletionIntervalValidator _intervalValidator = intervalValidator;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public RestartTaskDisplayState GetInitialState(AppConfig config)
    {
        if (config == null)
        {
            return new RestartTaskDisplayState
            {
                ChosenBrowser = _localizationService.GetString("Task_DefaultBrowser"),
                PauseSeconds = 20,
                RuntimeSeconds = 3600
            };
        }

        string username = config.Username ?? "-";
        string chosenBrowser = config.Browser?.Selected ?? _localizationService.GetString("Task_DefaultBrowser");

        int runtimeSeconds = config.Browser != null ? config.Browser.RuntimeHours * 3600 : 3600;
        int pauseSeconds = config.Browser?.RuntimePauseSeconds ?? 20;

        bool checkBrowserAliveRoutine = config.Browser?.CheckBrowserAliveRoutine ?? false;

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
            {
                nextDate = today.AddDays(intervalDays);
            }

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
            ChosenBrowser = chosenBrowser,
            RuntimeSeconds = runtimeSeconds,
            PauseSeconds = pauseSeconds,
            CheckBrowserAliveRoutine = checkBrowserAliveRoutine,
            DeleteBrowserContentIsActive = deleteBrowserContentIsActive,
            DeleteIsActivatedMessage = deleteIsActivatedMessage,
            NextDeletionProcessMessage = nextDeletionProcessMessage,
            NextDeletionProcessDateMessage = nextDeletionProcessDateMessage
        };
    }
}
