using System;
using System.Collections.Generic;
using System.Text;

namespace SmartMetroService.Application.Models;

public class QrCodeRequest
{
    public string QrCode { get; set; } = string.Empty;
    public int StationId { get; set; } 
    public string Gate { get; set; } = string.Empty;
}
