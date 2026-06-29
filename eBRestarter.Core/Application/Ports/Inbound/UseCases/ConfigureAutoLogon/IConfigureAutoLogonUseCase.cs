using FluentResults;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ConfigureAutoLogon;

public interface IConfigureAutoLogonUseCase
{
    Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request);
}

