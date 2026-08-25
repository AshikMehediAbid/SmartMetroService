using SmartMetroService.Application.Models;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IScannerService
{
    Task ValidateQrData(QrCodeRequest qrData);
}
