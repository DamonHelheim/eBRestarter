using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for resolving application-specific Windows file system paths.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Bereitstellung von OS-spezifischen Verzeichnispfaden.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortAppPathProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt und einen Outbound Port implementiert, um Systempfade bereitzustellen.
/// </para>
/// </summary>
public sealed class AdapterWindowsAppPathProvider : IOutboundPortAppPathProvider
{
    public string RetrieveAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public string RetrieveLocalAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string RetrieveUserProfileDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public string RetrieveProgramFilesDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public string RetrieveProgramFilesX86Directory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
}





