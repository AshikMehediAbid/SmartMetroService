namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
