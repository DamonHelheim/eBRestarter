namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
public interface IOsAutoLogonPort
{
    void EnableAutoLogon(string username, string domain, string password);
    bool IsPasswordlessAuthEnabled();
    void SetPasswordlessAuth(bool enable);
    void DisableAutoLogon();
    bool IsAutoLogonEnabled();
}
