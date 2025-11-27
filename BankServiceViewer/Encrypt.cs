using System;
using System.Security.Cryptography;
using System.Text;

namespace BankServiceViewer;

internal static class Encrypt
{
    public static string EncryptString(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return string.Empty;
        }

        const string passphrase = "s1e4c5r3eit";
        byte[] results;
        UTF8Encoding utf8 = new();

        using var hashProvider = new MD5CryptoServiceProvider();
        byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));

        using var tdesAlgorithm = new TripleDESCryptoServiceProvider
        {
            Key = tdesKey,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };

        ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
        byte[] dataToEncrypt = utf8.GetBytes(message);
        results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);

        return Convert.ToBase64String(results);
    }

    public static string DecryptString(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return string.Empty;
        }

        const string passphrase = "s1e4c5r3eit";
        UTF8Encoding utf8 = new();

        using var hashProvider = new MD5CryptoServiceProvider();
        byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));

        using var tdesAlgorithm = new TripleDESCryptoServiceProvider
        {
            Key = tdesKey,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };

        ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
        byte[] dataToDecrypt = Convert.FromBase64String(message);
        byte[] results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);

        return utf8.GetString(results);
    }
}
