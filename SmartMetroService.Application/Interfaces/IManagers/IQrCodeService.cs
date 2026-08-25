namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string text);
}
