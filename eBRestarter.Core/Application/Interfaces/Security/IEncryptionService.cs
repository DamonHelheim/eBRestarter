using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
