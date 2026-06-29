namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleAppAutoStart;

public interface IToggleAppAutoStartUseCase
{
    Task<bool> InitializeAndGetStateAsync();
    Task ToggleAsync(bool enable);
}

