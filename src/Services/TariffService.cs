using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EasyParking.Models;

namespace EasyParking.Services;

public class TariffService
{
    private readonly EasyParkingDbContext _dbContext;

    public TariffService(EasyParkingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
            
            // Apply CHF 35 daily cap
            if (dailyCost > 35m)
                dailyCost = 35m;
                
            totalCost += dailyCost;
            
            currentDay = currentDay.AddDays(1);
        }

        return totalCost;
    }

    private decimal CalculateDailyCost(DateTime start, DateTime end, System.Collections.Generic.List<Tariff> tariffs)
    {
        decimal dailyCost = 0;
        DayType dayType = (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday) ? DayType.Weekend : DayType.Weekday;

        var dailyTariffs = tariffs.Where(t => t.DayType == dayType).ToList();

        // Calculate in 15-minute intervals
        DateTime current = start;
        while (current < end)
        {
            TimeSpan currentTimeOfDay = current.TimeOfDay;
            var applicableTariff = dailyTariffs.FirstOrDefault(t => t.StartTime <= currentTimeOfDay && (t.EndTime > currentTimeOfDay || t.EndTime == TimeSpan.FromHours(24)));
            
            if (applicableTariff != null)
            {
                dailyCost += applicableTariff.RatePerHour / 4m;
            }

            current = current.AddMinutes(15);
        }

        return dailyCost;
    }
}
