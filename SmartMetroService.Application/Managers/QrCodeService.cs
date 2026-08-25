using QRCoder;
using SmartMetroService.Application.Interfaces.IManagers;

namespace SmartMetroService.Application.Managers;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(10);
    }
}
