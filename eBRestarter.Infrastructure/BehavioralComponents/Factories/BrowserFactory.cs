using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

namespace eBRestarter.Infrastructure.BehavioralComponents.Factories;

/// <summary>
/// Factory for resolving and instantiating concrete <see cref="IOutboundPortBrowser"/> implementations.
/// </summary>
/// <param name="serviceProvider">The service provider used to resolve keyed and typed browser wrapper instances.</param>
public sealed class BrowserFactory(
    IServiceProvider serviceProvider)
    : IOutboundPortBrowserFactory
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string UnsupportedBrowserErrorMessagePattern = "Browser '{0}' is not supported by {1}.";

    // ⚡ Guide Kap. 9.2 (Werttyp in object-Parameter): GetKeyedService nimmt den Schlüssel als
    // object entgegen – ein direkt übergebener BrowserType würde bei JEDEM Aufruf geboxt.
    // Die Boxen werden hier einmalig erzeugt und danach wiederverwendet.
    private static readonly FrozenDictionary<BrowserType, object> BoxedBrowserTypeKeys =
        Enum.GetValues<BrowserType>()
            .ToFrozenDictionary(
                static browserType => browserType,
                static browserType => (object)browserType,
                EqualityComparer<BrowserType>.Default);


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public IOutboundPortBrowser Create(BrowserType type)
    {
        if (BoxedBrowserTypeKeys.TryGetValue(type, out object? cachedKey)
            && _serviceProvider.GetKeyedService<IOutboundPortBrowser>(cachedKey) is { } keyedBrowser)
        {
            return keyedBrowser;
        }

        return type switch
        {
            BrowserType.Chrome => _serviceProvider.GetRequiredService<AdapterChromeBrowserWrapper>(),
            BrowserType.Firefox => _serviceProvider.GetRequiredService<AdapterFirefoxBrowserWrapper>(),
            BrowserType.Edge => _serviceProvider.GetRequiredService<AdapterEdgeBrowserWrapper>(),
            BrowserType.Brave => _serviceProvider.GetRequiredService<AdapterBraveBrowserWrapper>(),
            BrowserType.Vivaldi => _serviceProvider.GetRequiredService<AdapterVivaldiBrowserWrapper>(),
            _ => throw new NotSupportedException(string.Format(UnsupportedBrowserErrorMessagePattern, type, nameof(BrowserFactory)))
        };
    }
}
