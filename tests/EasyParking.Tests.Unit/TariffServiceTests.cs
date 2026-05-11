using EasyParking.Models;
using EasyParking.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EasyParking.Tests.Unit;

public class TariffServiceTests : IDisposable
{
    private readonly EasyParkingDbContext _context;
    private readonly TariffService _service;

    public TariffServiceTests()
    {
        var options = new DbContextOptionsBuilder<EasyParkingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EasyParkingDbContext(options);
        SeedData();
        _service = new TariffService(_context);
    }

    private void SeedData()
    {
        _context.Tariffs.AddRange([
            new Tariff { Id = 1, DayType = DayType.Weekday, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(6, 0, 0), RatePerHour = 2.50m },
            new Tariff { Id = 2, DayType = DayType.Weekday, StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(9, 0, 0), RatePerHour = 2.80m },
            new Tariff { Id = 3, DayType = DayType.Weekday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0), RatePerHour = 3.60m },
            new Tariff { Id = 4, DayType = DayType.Weekday, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(21, 0, 0), RatePerHour = 2.80m },
            new Tariff { Id = 5, DayType = DayType.Weekday, StartTime = new TimeSpan(21, 0, 0), EndTime = new TimeSpan(24, 0, 0), RatePerHour = 2.40m },
            new Tariff { Id = 6, DayType = DayType.Weekend, StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(9, 0, 0), RatePerHour = 2.40m },
            new Tariff { Id = 7, DayType = DayType.Weekend, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0), RatePerHour = 3.20m },
            new Tariff { Id = 8, DayType = DayType.Weekend, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(24, 0, 0), RatePerHour = 2.40m }
        ]);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CalculateCostAsync_ShouldReturnZero_WhenExitTimeBeforeEntryTime()
    {
        var entry = new DateTime(2026, 5, 4, 10, 0, 0); // Montag
        var exit = entry.AddHours(-1);

        var cost = await _service.CalculateCostAsync(entry, exit);

        Assert.Equal(0, cost);
    }

    [Fact]
    public async Task CalculateCostAsync_ShouldCalculateSimpleWeekdayTariff()
    {
        // Montag 10:00 bis 11:00 -> Tarif 3 (3.60/h)
        var entry = new DateTime(2026, 5, 4, 10, 0, 0);
        var exit = entry.AddHours(1);

        var cost = await _service.CalculateCostAsync(entry, exit);

        Assert.Equal(3.60m, cost);
    }

    [Fact]
    public async Task CalculateCostAsync_ShouldCalculateAcrossTariffChanges()
    {
        // Montag 08:30 bis 09:30 
        // 08:30-09:00 (30 Min) -> Tarif 2 (2.80/h) -> 1.40
        // 09:00-09:30 (30 Min) -> Tarif 3 (3.60/h) -> 1.80
        // Gesamt: 3.20
        var entry = new DateTime(2026, 5, 4, 8, 30, 0);
        var exit = entry.AddHours(1);

        var cost = await _service.CalculateCostAsync(entry, exit);

        Assert.Equal(3.20m, cost);
    }

    [Fact]
    public async Task CalculateCostAsync_ShouldApplyDailyCap()
    {
        // Montag 00:00 bis 23:59 -> Sollte bei 35.00 gedeckelt sein
        // Ohne Deckelung: 6*2.5 + 3*2.8 + 9*3.6 + 3*2.8 + 3*2.4 = 15 + 8.4 + 32.4 + 8.4 + 7.2 = 71.40
        var entry = new DateTime(2026, 5, 4, 0, 0, 0);
        var exit = entry.AddHours(23).AddMinutes(59);

        var cost = await _service.CalculateCostAsync(entry, exit);

        Assert.Equal(35.00m, cost);
    }

    [Fact]
    public async Task CalculateCostAsync_ShouldHandleWeekendTariff()
    {
        // Sonntag 10:00 bis 11:00 -> Tarif 7 (3.20/h)
        var entry = new DateTime(2026, 5, 3, 10, 0, 0);
        var exit = entry.AddHours(1);

        var cost = await _service.CalculateCostAsync(entry, exit);

        Assert.Equal(3.20m, cost);
    }

    [Fact]
    public async Task CalculateCostAsync_ShouldHandleLongTermParking()
    {
        // Mehr als 24 Stunden -> 35.00 pro Tag (Aufrundung)
        var entry = new DateTime(2026, 5, 4, 10, 0, 0);
        var exit = entry.AddHours(25); // 1 Tag und 1 Stunde -> Verrechnung von 2 Tagen

        var cost = await _service.CalculateCostAsync(entry, exit);

        Assert.Equal(70.00m, cost);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
