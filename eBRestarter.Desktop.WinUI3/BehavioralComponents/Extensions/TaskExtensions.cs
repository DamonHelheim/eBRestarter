using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions;

/// <summary>
/// Helpers for fire-and-forget tasks started from the UI layer.
/// </summary>
/// <remarks>
/// 📝 <b>Warum diese Klasse aus dem Core hierher gewandert ist:</b> Sie lag in
/// <c>eBRestarter.Core.Application.BehavioralComponents.Extensions</c>, obwohl <b>alle</b> sieben
/// Aufrufer im Desktop-Layer liegen (Konstruktoren und Property-Setter von ViewModels, die keine
/// asynchrone Signatur haben können). Damit die Guideline-Regel aus Kap. 2 — Logger per DI, kein
/// <c>Debug.WriteLine</c> — hier überhaupt umsetzbar ist, braucht der Helfer Zugriff auf
/// <c>ILogger</c>. Im Core wäre das nur um den Preis einer Framework-Abhängigkeit gegangen, die
/// die Hexagonal-Vorgabe dieses Projekts verbietet. Der Umzug löst beides auf einmal: Der Core
/// bleibt framework-frei, und der Helfer bekommt ein ordentliches Log.
/// </remarks>
public static class TaskExtensions
{
    /// <summary>
    /// Observes a fire-and-forget <see cref="Task"/> so its exceptions cannot go unhandled.
    /// </summary>
    /// <param name="task">The task to observe.</param>
    /// <param name="logger">Logger of the calling component; receives any fault of <paramref name="task"/>.</param>
    /// <param name="context">
    /// Short description of what was started, e.g. <c>nameof(LoadBrowsersSmartAsync)</c>. Appears
    /// as a named placeholder value in the log entry (Kap. 4/12) — never as part of the template.
    /// </param>
    /// <remarks>
    /// <para>
    /// <b>Bewusste Abweichung von Performance-Guide Kap. 11.4</b> ("<c>async void</c> ist <b>nur</b>
    /// für Event-Handler erlaubt. In allen anderen Fällen <b>immer</b> <c>async Task</c> oder
    /// <c>async ValueTask</c> verwenden. <c>async void</c> verschluckt Exceptions und macht das
    /// Warten auf Completion unmöglich.").
    /// </para>
    /// <para>
    /// Von den beiden dort genannten Gründen greift nur einer:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <i>"verschluckt Exceptions"</i> — hier adressiert: der <c>catch</c>-Block unten beobachtet
    /// den Task und protokolliert jeden Fehler über <see cref="ILogger"/>. Ohne diesen Wrapper
    /// würde ein verworfener Task die Exception tatsächlich unbeobachtet lassen.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <i>"nicht abwartbar"</i> — trifft zu und ist die beabsichtigte Semantik dieses Helfers.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// <b>Aufrufer, die auf die Fertigstellung warten müssen, dürfen <c>Forget()</c> nicht
    /// verwenden</b> — dort gehört ein reguläres <c>await</c> hin.
    /// </para>
    /// </remarks>
#pragma warning disable S3168 // "async" methods should not return "void" - siehe <remarks> oben
    public static async void Forget(this Task task, ILogger logger, string context)
#pragma warning restore S3168
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // 📝 Logging-Guideline Kap. 2: vorher Debug.WriteLine - im Release-Build also
            // vollstaendig wirkungslos. Ein unbeobachteter Fehler in einem Fire-and-Forget-Task
            // war damit in Produktion unsichtbar.
            logger.LogError(
                LogEventIds.RestarterCycle.FireAndForgetTaskFaulted,
                exception,
                "Fire-and-forget task {Context} faulted.",
                context);
        }
    }
}
