using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Models;

namespace SmartMetroService.Api.Controllers;

[Route("api/scanner")]
[ApiController]
public class ScannerController : ControllerBase
{
    private readonly IScannerService _scannerService;

    public ScannerController(IScannerService scannerService)
    {
        _scannerService = scannerService;
    }

    [HttpPost]
    [Route("qr-data")]
    public async Task<IActionResult> QrTicketScanner([FromBody] QrCodeRequest request) // C3LNFzbPsys1NS30X5j8BippKuMc48LW1hjmEeSOtWkdPpcLl5Qh0mkt6a4zHp+V
    {
        try
        {
            await _scannerService.ValidateQrData(request);

            return Ok(new { Message = "Gate Pass. Thank You!" });
        }
        catch(Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
