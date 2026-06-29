using FluentResults;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ConfigureAutoLogon;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IConfigureAutoLogonUseCase
{
    Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request);
}

