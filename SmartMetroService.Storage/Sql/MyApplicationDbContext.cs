using Microsoft.EntityFrameworkCore;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Storage.Sql;

public class MyApplicationDbContext : DbContext
{
    public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserOTP> UserTokens { get; set; }
    public DbSet<Token> Tokens { get; set; }
}
