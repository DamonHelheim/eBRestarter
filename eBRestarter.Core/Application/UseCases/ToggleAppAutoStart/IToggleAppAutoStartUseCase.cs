namespace eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;

public interface IToggleAppAutoStartUseCase
{
    Task<bool> InitializeAndGetStateAsync();
    Task ToggleAsync(bool enable);
}
