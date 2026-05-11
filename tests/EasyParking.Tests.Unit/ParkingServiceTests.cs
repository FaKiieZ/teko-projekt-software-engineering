using EasyParking.Models;
using EasyParking.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EasyParking.Tests.Unit;

public class ParkingServiceTests : IDisposable
{
    private readonly EasyParkingDbContext _context;
    private readonly ParkingService _service;

    public ParkingServiceTests()
    {
        var options = new DbContextOptionsBuilder<EasyParkingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EasyParkingDbContext(options);
        SeedData();
        _service = new ParkingService(_context);
    }

    private void SeedData()
    {
        var garage = new ParkingGarage { Id = 1, Name = "Test Garage" };
        _context.ParkingGarages.Add(garage);

        var floor1 = new Floor { Id = 1, Number = 1, ParkingGarageId = 1 };
        var floor2 = new Floor { Id = 2, Number = 2, ParkingGarageId = 1 };
        _context.Floors.AddRange(floor1, floor2);

        // Etage 1: 1 freier Parkplatz
        _context.ParkingSpaces.Add(new ParkingSpace { Id = 1, Number = 101, FloorId = 1, IsOccupied = false });
        
        // Etage 2: 2 freie Parkplätze
        _context.ParkingSpaces.Add(new ParkingSpace { Id = 2, Number = 201, FloorId = 2, IsOccupied = false });
        _context.ParkingSpaces.Add(new ParkingSpace { Id = 3, Number = 202, FloorId = 2, IsOccupied = false });

        // Dem Mieter zugewiesener Parkplatz
        _context.Customers.Add(new Customer { Id = 10, Code = "TENANT_A", CustomerType = CustomerType.Tenant, IsActive = true });
        _context.ParkingSpaces.Add(new ParkingSpace { Id = 4, Number = 301, FloorId = 1, IsOccupied = false, AssignedTenantId = 10 });

        _context.SaveChanges();
    }

    [Fact]
    public async Task AssignFreeSpaceAsync_ShouldChooseFloorWithMostFreeSpaces()
    {
        // Etage 2 hat 2 freie Plätze, Etage 1 hat 1 (ohne Mieterplatz).
        var ticket = await _service.AssignFreeSpaceAsync();

        Assert.NotNull(ticket);
        Assert.Equal(2, ticket.FloorNumber);
        
        var space = _context.ParkingSpaces.First(ps => ps.Number == ticket.SpaceNumber && ps.FloorId == 2);
        Assert.True(space.IsOccupied);
    }

    [Fact]
    public async Task AssignFreeSpaceAsync_ShouldAssignTenantSpecificSpace()
    {
        var ticket = await _service.AssignFreeSpaceAsync(tenantId: 10);

        Assert.NotNull(ticket);
        Assert.Equal(1, ticket.FloorNumber);
        Assert.Equal(301, ticket.SpaceNumber);
        Assert.Equal(10, ticket.CustomerId);
    }

    [Fact]
    public async Task FreeSpaceAsync_ShouldMakeSpaceAvailableAndSetExitTime()
    {
        // Zuerst einen Platz zuweisen
        var ticket = await _service.AssignFreeSpaceAsync();
        Assert.NotNull(ticket);
        
        var exitTime = DateTime.Now.AddHours(2);
        var result = await _service.FreeSpaceAsync(ticket.Id, exitTime);

        Assert.True(result);
        
        var updatedTicket = _context.Tickets.Find(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.Equal(exitTime, updatedTicket.ExitTime);

        var space = _context.ParkingSpaces.First(ps => ps.Number == ticket.SpaceNumber && ps.FloorId == ticket.FloorNumber);
        Assert.False(space.IsOccupied);
    }

    [Fact]
    public async Task AssignFreeSpaceAsync_ShouldReturnNull_WhenGarageIsFull()
    {
        // Alle Plätze belegen
        var spaces = _context.ParkingSpaces.Where(ps => ps.AssignedTenantId == null).ToList();
        foreach (var s in spaces) s.IsOccupied = true;
        await _context.SaveChangesAsync();

        var ticket = await _service.AssignFreeSpaceAsync();

        Assert.Null(ticket);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
