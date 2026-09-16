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
    public DbSet<UserWallet> UserWallets { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Journey> Journeys { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<RapidPass> RapidPasses { get; set; }


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

        ConfigureTicket(modelBuilder);
        ConfigureJourney(modelBuilder);
        ConfigurePayment(modelBuilder);
    }


    private static void ConfigureTicket(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Ticket>();

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Fare)
            .IsRequired();

        entity.Property(x => x.TicketType)
            .IsRequired();

        entity.Property(x => x.TicketStatus)
            .IsRequired();

        entity.HasOne(x => x.FromStation)
            .WithMany(x => x.FromTickets)
            .HasForeignKey(x => x.FromStationId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.ToStation)
            .WithMany(x => x.ToTickets)
            .HasForeignKey(x => x.ToStationId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(x => x.UserId);

        entity.HasIndex(x => x.TicketStatus);

        entity.HasIndex(x => x.ExpiryTime);
    }


    private static void ConfigureJourney(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Journey>();

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Fare)
            .IsRequired();

        entity.Property(x => x.StartAt)
            .IsRequired();

        entity.Property(x => x.JourneyStatus)
            .IsRequired();

        // Journey -> Ticket
        entity.HasOne(x => x.Ticket)
            .WithMany(x => x.Journeys)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        // Journey -> From Station
        entity.HasOne(x => x.FromStation)
            .WithMany()
            .HasForeignKey(x => x.FromStationId)
            .OnDelete(DeleteBehavior.NoAction);

        // Journey -> To Station
        entity.HasOne(x => x.ToStation)
            .WithMany()
            .HasForeignKey(x => x.ToStationId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(x => x.UserId);

        entity.HasIndex(x => x.TicketId);

        entity.HasIndex(x => x.JourneyStatus);

        entity.HasIndex(x => x.StartAt);
    }

    private static void ConfigurePayment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Payment>();

        entity.HasKey(x => x.Id);
        entity.Property(x => x.MerTxnId).IsRequired();
        entity.Property(x => x.Currency).IsRequired();
        entity.Property(x => x.Status).IsRequired();
        entity.HasIndex(x => x.MerTxnId).IsUnique();
        entity.HasIndex(x => x.UserId);
    }

}
