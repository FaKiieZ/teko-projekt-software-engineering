namespace EasyParking.Models;

public class ParkingSpace
{
    public int Id { get; set; }
    public int Number { get; set; }
    
    public int FloorId { get; set; }
    public Floor? Floor { get; set; }

    public bool IsOccupied { get; set; }

    public int? AssignedTenantId { get; set; }
    public Customer? AssignedTenant { get; set; }
}
