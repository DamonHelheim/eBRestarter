namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

/// <summary>
/// Port: Driven Port (Outbound) for verifying that a downloaded executable carries a valid,
/// trusted code signature before it is launched.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Signaturprüfung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core bzw. in Update-/Installations-Adaptern, die fremde Binaries ausführen.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores im Infrastructure Layer (Windows Authenticode via <c>WinVerifyTrust</c>).<br/>
/// - <strong>Begründung:</strong> Entkoppelt den Anwendungskern von der plattformspezifischen Vertrauensprüfung und ist damit ein klassischer <strong>Outbound Port</strong>.
/// </para>
/// </summary>
/// <remarks>
/// 🔒 Security-Guideline Kap. 7.3 (Software Supply Chain Failures, OWASP A03/A08:2025):
/// Ein heruntergeladenes Installationspaket ist ein nicht vertrauenswürdiges Artefakt, bis seine
/// Herkunft nachgewiesen ist. Ohne diese Prüfung wird jede kompromittierte Release-Quelle und
/// jede untergeschobene Datei im Zielverzeichnis unmittelbar zu Codeausführung.
/// </remarks>
public interface IOutboundPortExecutableSignatureVerifier
{
    /// <summary>
    /// Determines whether the file carries a valid code signature that chains to a trusted root.
    /// </summary>
    /// <param name="filePath">Absolute path to the executable or installer package to verify.</param>
    /// <returns>
    /// <see langword="true"/> only if the signature is present, intact and trusted;
    /// <see langword="false"/> for unsigned, tampered, expired-chain or otherwise untrusted files.
    /// </returns>
    bool IsTrustedPublisher(string filePath);
}
