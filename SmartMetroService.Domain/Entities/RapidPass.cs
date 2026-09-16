namespace SmartMetroService.Domain.Entities;

public class RapidPass : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[]? QRByte { get; set; }
    public DateTime? ExpiryTime { get; set; }
}
