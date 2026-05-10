using System.Collections.Generic;

namespace EasyParking.Models;

public class Customer
{
    public int Id { get; set; }
    
    // Code für Dauermieter
    public string? Code { get; set; }
    
    public CustomerType CustomerType { get; set; }
    
    // Für Dauermieter zur Prüfung, ob die Miete bezahlt ist
    public bool IsActive { get; set; }

    public ICollection<ParkingSpace> ParkingSpaces { get; set; } = new List<ParkingSpace>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
