using System;

namespace EasyParking.Models;

public class Tariff
{
    public int Id { get; set; }
    public DayType DayType { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal RatePerHour { get; set; }
}
