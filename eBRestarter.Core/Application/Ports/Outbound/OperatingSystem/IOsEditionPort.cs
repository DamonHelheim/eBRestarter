namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

// Ergänzung zum bestehenden Registry-Service für WMI-spezifische OS-Daten
public interface IOsEditionPort
{
    Task<string> RetrieveOsEditionAsync();
}


