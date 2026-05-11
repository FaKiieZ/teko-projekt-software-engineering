namespace EasyParking.Models;

public class Ticket
{
    public int Id { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int FloorNumber { get; set; }
    public int SpaceNumber { get; set; }
    
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public decimal? Cost { get; set; }
    public bool IsPaid { get; set; }
}
