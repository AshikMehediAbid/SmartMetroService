using SmartMetroService.Domain.Enums;
using System.Security.Principal;

namespace SmartMetroService.Domain.Entities;

public class User : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string HashedPassword { get; set; }
    public UserRole UserRole { get; set; } = UserRole.User;
    public bool IsEmailVerified { get; set; } = false;
    public Guid? KeycloakUserId { get; set; } = null;
}
