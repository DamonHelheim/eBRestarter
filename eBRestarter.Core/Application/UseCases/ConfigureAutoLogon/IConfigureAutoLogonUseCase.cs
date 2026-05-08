using FluentResults;

namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public interface IConfigureAutoLogonUseCase
{
    Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request);
}
