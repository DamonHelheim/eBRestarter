namespace eBRestarter.Core.Application.Ports.Outbound.Security;

public interface IEncryptionOutboundPort
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
