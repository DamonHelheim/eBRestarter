namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.RemoveApiCredentials;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IRemoveApiCredentialsUseCase
{
    void Execute();
}

