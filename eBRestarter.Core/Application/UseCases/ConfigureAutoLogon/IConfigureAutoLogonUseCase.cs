namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public interface IConfigureAutoLogonUseCase
{
    ConfigureAutoLogonResponse Execute(ConfigureAutoLogonRequest request);
}