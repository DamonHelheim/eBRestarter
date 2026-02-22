namespace eBRestarter.Infrastructure.Wrapper.Interface;

// 1. Die Abstraktion für den Prozess-Pfad
public interface IProcessInfoService
{
    string GetCurrentExecutablePath();
}
