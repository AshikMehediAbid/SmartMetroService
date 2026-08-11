namespace SmartMetroService.Domain.Entities;

public class UserOTP
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Otp { get; set; }
    public OtpType Type { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; } = false;
}

public enum OtpType
{
    PASSWORD_RECOVERY = 0,
    EMAIL_VERIFICATION = 1,
    PasswordReset = 2
}
