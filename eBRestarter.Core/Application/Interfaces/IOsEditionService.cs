namespace eBRestarter.Core.Application.Interfaces;

// Ergänzung zum bestehenden Registry-Service für WMI-spezifische OS-Daten
public interface IOsEditionService
{
    Task<string> GetOsEditionAsync();
}
