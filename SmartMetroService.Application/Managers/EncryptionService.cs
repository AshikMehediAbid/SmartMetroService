using Microsoft.Extensions.Options;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;
using System.Security.Cryptography;
using System.Text;

namespace SmartMetroService.Application.Managers;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IOptions<EncryptionSettings> options)
    {
        _key = Encoding.UTF8.GetBytes(options.Value.Key);
        _iv = Encoding.UTF8.GetBytes(options.Value.IV);
    }
    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();

        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();

        var cipherBytes =
            Convert.FromBase64String(cipherText);

        var decryptedBytes =
            decryptor.TransformFinalBlock(
                cipherBytes,
                0,
                cipherBytes.Length);

        var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

        return decryptedText;
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();

        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        var encryptedBytes =
            encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length);

        var encryptedText = Convert.ToBase64String(encryptedBytes);
        return encryptedText;
    }
}
