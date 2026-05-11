using Microsoft.EntityFrameworkCore;
using EasyParking.Models;

namespace EasyParking.Services;

public class ParkingService(EasyParkingDbContext dbContext)
{
    private readonly EasyParkingDbContext _dbContext = dbContext;

    public async Task<Ticket?> AssignFreeSpaceAsync(int? tenantId = null)
    {
        var floors = await _dbContext.Floors
            .Include(f => f.ParkingSpaces)
            .ToListAsync();

        ParkingSpace? spaceToAssign = null;
        Floor? selectedFloor = null;

        if (tenantId.HasValue)
        {
            foreach (var f in floors)
            {
                var space = f.ParkingSpaces.FirstOrDefault(ps => ps.AssignedTenantId == tenantId);
                if (space != null)
                {
                    spaceToAssign = space;
                    selectedFloor = f;
                    break;
                }
            }
        }
        else
        {
            var floorWithMostFreeSpaces = floors
                .OrderByDescending(f => f.ParkingSpaces.Count(ps => !ps.IsOccupied && ps.AssignedTenantId == null))
                .FirstOrDefault();

            if (floorWithMostFreeSpaces != null)
            {
                spaceToAssign = floorWithMostFreeSpaces.ParkingSpaces.FirstOrDefault(ps => !ps.IsOccupied && ps.AssignedTenantId == null);
                selectedFloor = floorWithMostFreeSpaces;
            }
        }

        if (spaceToAssign == null || selectedFloor == null)
            return null; // Parkhaus voll oder kein zugewiesener Platz gefunden

        spaceToAssign.IsOccupied = true;

        var ticket = new Ticket
        {
            EntryTime = DateTime.Now,
            FloorNumber = selectedFloor.Number,
            SpaceNumber = spaceToAssign.Number,
            CustomerId = tenantId,
            IsPaid = false
        };

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync();

        return ticket;
    }

    public async Task<bool> FreeSpaceAsync(int ticketId, DateTime exitTime)
    {
        var ticket = await _dbContext.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return false;

        var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Number == ticket.FloorNumber);
        if (floor == null)
            return false;

        var space = await _dbContext.ParkingSpaces.FirstOrDefaultAsync(ps => ps.FloorId == floor.Id && ps.Number == ticket.SpaceNumber);
        if (space != null)
        {
            space.IsOccupied = false;
        }

        ticket.ExitTime = exitTime;
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
