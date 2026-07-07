using eBRestarter.Core.Application.Common.Results;

namespace eBRestarter.Core.Application.Ports.Inbound.Validators;

/// <summary>
/// Port: Driven Port (Outbound) for executing application-level object validation through an external validation engine.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Validierungs-Infrastruktur)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.UseCases.DeleteBrowserContentUseCase"/>, <see cref="eBRestarter.Core.Application.Services.RestarterCycleService"/> etc.).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Validators.FluentValidationAdapter{T}"/> im Infrastructure Layer via FluentValidation).<br/>
/// - <strong>Begründung:</strong> Da der Core diese Schnittstelle aufruft und die Implementierung im Infrastruktur-Layer ein externes Framework gekapselt anbietet, handelt es sich nach Leitfaden Abschnitt 1 zwingend um einen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Der Name <c>IInboundPortApplicationValidator</c> und der Ablageort in <c>Ports/Inbound/Validators/</c> sind architektonisch fehlerhaft. Die Schnittstelle sollte auf <c>IApplicationValidatorOutboundPort</c> (oder <c>IApplicationValidator</c>) umbenannt und nach <c>Ports/Outbound/</c> verschoben werden.
/// </para>
/// </summary>
public interface IInboundPortApplicationValidator<in T>
{
    ValidationResult Validate(T request);
}
