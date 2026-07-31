namespace SmartMetroService.Domain.Entities;

public class Token : BaseEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddMonths(1);
    public DateTime? RevokedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

}
