using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Common.Results;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;

/// <summary>
/// Port: Use Case Interface for configuring OS automatic logon settings from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsGeneral"/> im Presentation Layer).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="Application.UseCases.ConfigureAutoLogonUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um den Use Case zur Konfiguration des automatischen Windows-Logons auszuführen. Gemäß Leitfaden ist das Suffix <c>UseCase</c> für diese Inbound-Rolle vorbildlich.
/// </para>
/// </summary>
public interface IUseCaseConfigureAutoLogon
{
    Result<AutoLogonResultStatus> Execute(ConfigureAutoLogonRequest request);
}

