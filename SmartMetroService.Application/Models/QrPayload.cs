using System;
using System.Collections.Generic;
using System.Text;

namespace SmartMetroService.Application.Models;

public class QrPayload
{
    public Guid Id { get; set; }
    public DateTime ExpiryTime { get; set; }
    public string QrType { get; set; } = string.Empty;
}
