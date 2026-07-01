namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

// Erg�nzung zum bestehenden Registry-Service f�r WMI-spezifische OS-Daten
public interface IOsEditionProviderOutboundPort
{
    Task<string> RetrieveOsEditionAsync();
}


