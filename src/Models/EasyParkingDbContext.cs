using Microsoft.EntityFrameworkCore;

namespace EasyParking.Models;

public class EasyParkingDbContext : DbContext
{
    public EasyParkingDbContext() { }

    public EasyParkingDbContext(DbContextOptions<EasyParkingDbContext> options)
        : base(options) { }

    public DbSet<ParkingGarage> ParkingGarages { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<ParkingSpace> ParkingSpaces { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Tariff> Tariffs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=easyparking.db");
        }
    }

    // Standard-Tarife gemäss Anforderungen initial befüllen
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tariff>().HasData([
            // Werktag-Tarife
            new Tariff { Id = 1, DayType = DayType.Weekday, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(6, 0, 0), RatePerHour = 2.50m },
            new Tariff { Id = 2, DayType = DayType.Weekday, StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(9, 0, 0), RatePerHour = 2.80m },
            new Tariff { Id = 3, DayType = DayType.Weekday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0), RatePerHour = 3.60m },
            new Tariff { Id = 4, DayType = DayType.Weekday, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(21, 0, 0), RatePerHour = 2.80m },
            new Tariff { Id = 5, DayType = DayType.Weekday, StartTime = new TimeSpan(21, 0, 0), EndTime = new TimeSpan(24, 0, 0), RatePerHour = 2.40m },

            // Wochenend-Tarife
            new Tariff { Id = 6, DayType = DayType.Weekend, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(9, 0, 0), RatePerHour = 2.40m },
            new Tariff { Id = 7, DayType = DayType.Weekend, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0), RatePerHour = 3.20m },
            new Tariff { Id = 8, DayType = DayType.Weekend, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(24, 0, 0), RatePerHour = 2.40m }
        ]);
    }
}
