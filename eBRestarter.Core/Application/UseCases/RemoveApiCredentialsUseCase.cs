using System;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for resetting and clearing stored eBesucher API credentials from the application configuration.
/// </summary>
/// <param name="configService">Outbound repository for accessing application configuration settings.</param>
public sealed class RemoveApiCredentialsUseCase(
    IOutboundPortEVisitorConfigRepository configService) : IUseCaseRemoveApiCredentials
{
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));

    /// <inheritdoc />
    public void Execute()
    {
        var currentConfig = _configService.LoadConfig();
        if (currentConfig.Settings is null)
        {
            return;
        }

        currentConfig.Settings.ApiUsername = string.Empty;
        currentConfig.Settings.ApiKey = string.Empty;

        _configService.SaveConfig(currentConfig);
    }
}
