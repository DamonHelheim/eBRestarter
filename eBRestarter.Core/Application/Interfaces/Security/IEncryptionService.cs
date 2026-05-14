namespace eBRestarter.Core.Application.Interfaces.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
