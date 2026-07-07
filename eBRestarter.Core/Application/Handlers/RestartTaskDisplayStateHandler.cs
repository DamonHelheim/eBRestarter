using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Application.Ports.Inbound.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Core.Domain.Validators;

namespace eBRestarter.Core.Application.Handlers;

/// <summary>
/// Erzeugt RestartTaskDisplayState aus Config und Lokalisierung.
/// </summary>
public class RestartTaskDisplayStateHandler(
    IInboundPortLocalizationProvider LocalizationService,
    ICacheDeletionIntervalValidator intervalValidator,
    TimeProvider timeProvider) : IInboundPortRestartTaskDisplayStateHandler
{
    private readonly IInboundPortLocalizationProvider _localizationService = LocalizationService;
    private readonly ICacheDeletionIntervalValidator _intervalValidator = intervalValidator;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public RestartTaskDisplayState RetrieveInitialState(AppConfig config)
    {
        if (config is null)
        {
            return new RestartTaskDisplayState
            {
                ChosenBrowser = _localizationService.RetrieveString("Task_DefaultBrowser"),
                PauseSeconds = 20,
                RuntimeSeconds = 3600
            };
        }

        string username = config.Username ?? "-";
        string chosenBrowser = config.Browser?.Selected ?? _localizationService.RetrieveString("Task_DefaultBrowser");

        int runtimeSeconds = config.Browser is null ? 3600 : config.Browser.RuntimeHours * 3600;
        int pauseSeconds = config.Browser?.RuntimePauseSeconds ?? 20;

        bool checkBrowserAliveRoutine = config.Browser?.CheckBrowserAliveRoutine ?? false;

        int intervalDays = config.Browser?.DeleteBrowserCacheIntervalDays ?? 0;
        DateTime nextDate = config.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;

        var deleteBrowserContentIsActive = _intervalValidator.IsValidIntervalDays(intervalDays);

        string deleteIsActivatedMessage;
        string nextDeletionProcessMessage;
        string nextDeletionProcessDateMessage;

        if (deleteBrowserContentIsActive)
        {
            deleteIsActivatedMessage = _localizationService.RetrieveString("Activate");
            nextDeletionProcessMessage = _localizationService.RetrieveString("NextDeletionProcess");

            var formatPattern = _localizationService.RetrieveString("Browser_NextDeleteDate_Format");

            var today = _timeProvider.GetLocalNow().Date;

            if (nextDate == today && intervalDays > 0)
            {
                nextDate = today.AddDays(intervalDays);
            }

            nextDeletionProcessDateMessage = string.Format(formatPattern, nextDate);
        }
        else
        {
            deleteIsActivatedMessage = _localizationService.RetrieveString("Disabled");
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



