using Microsoft.EntityFrameworkCore;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Storage.Sql;

public class MyApplicationDbContext : DbContext
{
    public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserOTP> UserOtps { get; set; }
    public DbSet<Token> RefreshTokens { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<StationDistance> StationDistances { get; set; }
    public DbSet<Settings> Settings { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StationDistance>()
            .HasOne(sd => sd.FromStation)
            .WithMany(s => s.FromDistances)
            .HasForeignKey(sd => sd.FromStationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StationDistance>()
            .HasOne(sd => sd.ToStation)
            .WithMany(s => s.ToDistances)
            .HasForeignKey(sd => sd.ToStationId)
            .OnDelete(DeleteBehavior.NoAction);
    }

}
