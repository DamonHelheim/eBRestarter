namespace eBRestarter.Core.Application.Ports.Outbound.Security;

public interface IEncryptionPort
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
