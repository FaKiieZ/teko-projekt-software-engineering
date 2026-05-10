using Microsoft.EntityFrameworkCore;
using EasyParking.Models;

namespace EasyParking.Services;

public class TariffService(EasyParkingDbContext dbContext)
{
    private readonly EasyParkingDbContext _dbContext = dbContext;

    public async Task<decimal> CalculateCostAsync(DateTime entryTime, DateTime exitTime)
    {
        if (exitTime <= entryTime)
            return 0;

        if ((exitTime - entryTime).TotalHours > 24)
        {
            return Math.Ceiling((decimal)(exitTime - entryTime).TotalDays) * 35m;
        }

        var tariffs = await _dbContext.Tariffs.ToListAsync();
        
        decimal totalCost = 0;
        DateTime currentDay = entryTime.Date;

        while (currentDay <= exitTime.Date)
        {
            DateTime dayStart = currentDay == entryTime.Date ? entryTime : currentDay;
            DateTime dayEnd = currentDay == exitTime.Date ? exitTime : currentDay.AddDays(1);
            
            decimal dailyCost = CalculateDailyCost(dayStart, dayEnd, tariffs);
            
            // Tagespauschale von CHF 35 anwenden
            if (dailyCost > 35m)
                dailyCost = 35m;
                
            totalCost += dailyCost;
            
            currentDay = currentDay.AddDays(1);
        }

        return totalCost;
    }

    private decimal CalculateDailyCost(DateTime start, DateTime end, List<Tariff> tariffs)
    {
        decimal dailyCost = 0;
        DayType dayType = (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday) ? DayType.Weekend : DayType.Weekday;

        var dailyTariffs = tariffs.Where(t => t.DayType == dayType).ToList();

        // Berechnung in 15-Minuten-Intervallen
        DateTime current = start;
        while (current < end)
        {
            TimeSpan currentTimeOfDay = current.TimeOfDay;
            var applicableTariff = dailyTariffs
                .FirstOrDefault(t => t.StartTime <= currentTimeOfDay
                    && (t.EndTime > currentTimeOfDay || t.EndTime == TimeSpan.FromHours(24)));

            if (applicableTariff != null)
            {
                dailyCost += applicableTariff.RatePerHour / 4m;
            }

            current = current.AddMinutes(15);
        }

        return dailyCost;
    }
}
