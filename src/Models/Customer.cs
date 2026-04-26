using System.Collections.Generic;

namespace EasyParking.Models;

public class Customer
{
    public int Id { get; set; }
    
    // Code for Dauermieter
    public string? Code { get; set; }
    
    public CustomerType CustomerType { get; set; }
    
    // For tenants to check if rent is paid
    public bool IsActive { get; set; }

    public ICollection<ParkingSpace> ParkingSpaces { get; set; } = new List<ParkingSpace>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
