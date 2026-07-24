using Microsoft.EntityFrameworkCore;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Storage.Sql;

public class MyApplicationDbContext : DbContext
{
    public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options) : base() { }

    public DbSet<User> Users { get; set; }
}
