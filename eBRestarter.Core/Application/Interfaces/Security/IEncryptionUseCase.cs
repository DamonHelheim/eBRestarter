namespace eBRestarter.Core.Application.Interfaces.Security;

public interface IEncryptionUseCase
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
