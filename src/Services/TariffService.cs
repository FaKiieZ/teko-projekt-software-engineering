using EasyParking.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyParking.Services;

public class TariffService(EasyParkingDbContext dbContext)
{
    private readonly EasyParkingDbContext _dbContext = dbContext;

    public async Task<decimal> CalculateCostAsync(DateTime entryTime, DateTime exitTime)
    {
        if (exitTime <= entryTime)
            return 0;

        TimeSpan duration = exitTime - entryTime;

        if (duration.TotalHours > 24)
        {
            // Ab 24 Stunden gilt die Tagespauschale pro angebrochenem Tag
            return Math.Ceiling((decimal)duration.TotalDays) * 35m;
        }

        var tariffs = await _dbContext.Tariffs.ToListAsync();
        
        decimal totalCost = 0;
        DateTime current = entryTime;

        while (current < exitTime)
        {
            DateTime nextDay = current.Date.AddDays(1);
            DateTime segmentEnd = nextDay < exitTime ? nextDay : exitTime;
            
            totalCost += CalculateDailyCost(current, segmentEnd, tariffs);
            current = segmentEnd;
        }

        // Bei einer Dauer bis 24 Stunden wird der Gesamtbetrag auf 35.00 begrenzt
        if (totalCost > 35m)
            totalCost = 35m;

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
