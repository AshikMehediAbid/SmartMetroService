namespace SmartMetroService.Domain.Entities;

public class UserWallet : BaseEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public double Balance { get; set; }
}
