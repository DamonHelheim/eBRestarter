namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

/// <summary>
/// Port: Driven Port (Outbound) for encrypting and decrypting sensitive data (e.g., credentials or configuration tokens).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Verschlüsselung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in der Anwendungs- und Infrastrukturlogik zur sicheren Ablage von Konfigurationswerten).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores im Infrastructure Layer via Windows DPAPI/Kryptografie-APIs).<br/>
/// - <strong>Begründung:</strong> Entkoppelt sensible Verschlüsselungsmechanismen vom Anwendungskern und stellt somit nach Leitfaden einen klassischen <strong>Outbound Port</strong> dar.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortEncryption
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
