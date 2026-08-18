using System;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.ImmutableSnapshot;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Domain.Validators;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

/// <summary>
/// Builds the initial <see cref="RestartTaskDisplayState"/> from application configuration and localization resources.
/// </summary>
/// <param name="intervalValidator">The validator used to check cache deletion interval bounds.</param>
/// <param name="localizationService">The provider for localized display string resources.</param>
/// <param name="timeProvider">The time provider for local time calculation.</param>
public sealed class RestartTaskDisplayStateHandler(
    ICacheDeletionIntervalValidator intervalValidator,
    IInboundPortLocalizationProvider localizationService,
    TimeProvider timeProvider) : IInboundPortRestartTaskDisplayStateHandler
{
    private const int DefaultPauseSeconds = 20;
    private const int DefaultRuntimeSeconds = 3600;
    private const int SecondsPerHour = 3600;
    private const string DefaultUsernameFallback = "-";

    private const string LocalizationKeyActivate = "Activate";
    private const string LocalizationKeyDisabled = "Disabled";
    private const string LocalizationKeyNextDeleteDateFormat = "Browser_NextDeleteDate_Format";
    private const string LocalizationKeyNextDeletionProcess = "NextDeletionProcess";
    private const string LocalizationKeyTaskDefaultBrowser = "Task_DefaultBrowser";

    private readonly ICacheDeletionIntervalValidator _intervalValidator = intervalValidator ?? throw new ArgumentNullException(nameof(intervalValidator));
    private readonly IInboundPortLocalizationProvider _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    
    /// <inheritdoc />
    public RestartTaskDisplayState RetrieveInitialState(AppConfig config)
    {
        if (config is null)
        {
            return new RestartTaskDisplayState
            {
                ChosenBrowser = _localizationService.RetrieveString(LocalizationKeyTaskDefaultBrowser),
                PauseSeconds = DefaultPauseSeconds,
                RuntimeSeconds = DefaultRuntimeSeconds
            };
        }

        var browser = config.Browser;
        string username = config.Username ?? DefaultUsernameFallback;
        string chosenBrowser = browser?.Selected ?? _localizationService.RetrieveString(LocalizationKeyTaskDefaultBrowser);

        int runtimeSeconds = browser is null ? DefaultRuntimeSeconds : browser.RuntimeHours * SecondsPerHour;
        int pauseSeconds = browser?.RuntimePauseSeconds ?? DefaultPauseSeconds;

        bool checkBrowserAliveRoutine = browser?.CheckBrowserAliveRoutine ?? false;
        int intervalDays = browser?.DeleteBrowserCacheIntervalDays ?? 0;
        DateTime nextDate = browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;

        bool deleteBrowserContentIsActive = _intervalValidator.IsValidIntervalDays(intervalDays);

        string deleteIsActivatedMessage;
        string nextDeletionProcessMessage;
        string nextDeletionProcessDateMessage;

        if (deleteBrowserContentIsActive)
        {
            deleteIsActivatedMessage = _localizationService.RetrieveString(LocalizationKeyActivate);
            nextDeletionProcessMessage = _localizationService.RetrieveString(LocalizationKeyNextDeletionProcess);

            string formatPattern = _localizationService.RetrieveString(LocalizationKeyNextDeleteDateFormat);
            DateTime today = _timeProvider.GetLocalNow().Date;

            if (nextDate == today && intervalDays > 0)
            {
                nextDate = today.AddDays(intervalDays);
            }

            nextDeletionProcessDateMessage = string.Format(formatPattern, nextDate);
        }
        else
        {
            deleteIsActivatedMessage = _localizationService.RetrieveString(LocalizationKeyDisabled);
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
