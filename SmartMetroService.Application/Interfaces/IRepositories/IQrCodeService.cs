namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string text);
}
