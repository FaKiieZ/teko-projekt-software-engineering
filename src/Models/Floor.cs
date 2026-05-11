namespace EasyParking.Models;

public class Floor
{
    public int Id { get; set; }
    public int Number { get; set; }
    public int TotalSpaces { get; set; }
    
    public int ParkingGarageId { get; set; }
    public ParkingGarage? ParkingGarage { get; set; }

    public ICollection<ParkingSpace> ParkingSpaces { get; set; } = new List<ParkingSpace>();
}
