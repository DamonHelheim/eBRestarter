namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleAppAutoStart;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IToggleAppAutoStartUseCase
{
    Task<bool> InitializeAndGetStateAsync();
    Task ToggleAsync(bool enable);
}

