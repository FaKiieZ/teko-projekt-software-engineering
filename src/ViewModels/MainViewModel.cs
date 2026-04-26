using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyParking.Models;
using EasyParking.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyParking.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly EasyParkingDbContext _dbContext;
    private readonly ParkingService _parkingService;
    private readonly TariffService _tariffService;
    private readonly System.Collections.Generic.Dictionary<int, DateTime> _paymentTimes = new();

    [ObservableProperty]
    private DateTime _simulatedTime;

    [ObservableProperty]
    private bool _entranceBarrierOpen;

    [ObservableProperty]
    private bool _exitBarrierOpen;

    [ObservableProperty]
    private string _currentTicketInfo = "Kein aktives Ticket";

    [ObservableProperty]
    private string _tenantCodeInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Floor> _floors = new();

    [ObservableProperty]
    private ObservableCollection<Ticket> _unpaidTickets = new();

    [ObservableProperty]
    private Ticket? _selectedTicketToPay;
    
    [ObservableProperty]
    private ObservableCollection<Ticket> _activeTickets = new();

    [ObservableProperty]
    private Ticket? _selectedTicketToExit;

    [ObservableProperty]
    private decimal _totalRevenue;

    public MainViewModel()
    {
        _dbContext = new EasyParkingDbContext();
        _parkingService = new ParkingService(_dbContext);
        _tariffService = new TariffService(_dbContext);
        
        SimulatedTime = DateTime.Now;

        InitializeDatabaseAsync();
    }

    private async void InitializeDatabaseAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync(); // Refresh for prototype to have a clean slate
        await _dbContext.Database.EnsureCreatedAsync();
        
        if (!await _dbContext.Floors.AnyAsync())
        {
            var garage = new ParkingGarage { Name = "Teko Garage" };
            _dbContext.ParkingGarages.Add(garage);
            await _dbContext.SaveChangesAsync();

            int[] floorSizes = { 15, 10, 5 };
            for (int i = 1; i <= 3; i++)
            {
                var floor = new Floor { Number = i, TotalSpaces = floorSizes[i - 1], ParkingGarageId = garage.Id };
                for (int j = 1; j <= floorSizes[i - 1]; j++)
                {
                    floor.ParkingSpaces.Add(new ParkingSpace { Number = j });
                }
                _dbContext.Floors.Add(floor);
            }

            // Seed a tenant
            var tenant = new Customer { Code = "1234", CustomerType = CustomerType.Tenant, IsActive = true };
            _dbContext.Customers.Add(tenant);
            
            await _dbContext.SaveChangesAsync();

            var firstFloorId = await _dbContext.Floors.Select(f => f.Id).FirstOrDefaultAsync();
            var firstSpace = await _dbContext.ParkingSpaces.FirstOrDefaultAsync(ps => ps.FloorId == firstFloorId && ps.Number == 1);
            if (firstSpace != null)
            {
                firstSpace.AssignedTenantId = tenant.Id;
                await _dbContext.SaveChangesAsync();
            }
        }

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var dbFloors = await _dbContext.Floors
            .Include(f => f.ParkingSpaces)
            .ThenInclude(ps => ps.AssignedTenant)
            .ToListAsync();
            
        Floors.Clear();
        foreach (var f in dbFloors)
            Floors.Add(f);

        var activeTix = await _dbContext.Tickets
            .Include(t => t.Customer)
            .Where(t => t.ExitTime == null)
            .ToListAsync();
            
        ActiveTickets.Clear();
        foreach (var t in activeTix)
            ActiveTickets.Add(t);

        var unpaidTix = activeTix.Where(t => !t.IsPaid && t.CustomerId == null).ToList();
        UnpaidTickets.Clear();
        foreach (var t in unpaidTix)
            UnpaidTickets.Add(t);
            
        TotalRevenue = await _dbContext.Tickets.SumAsync(t => t.Cost ?? 0);
    }

    [RelayCommand]
    private void AdvanceTime()
    {
        SimulatedTime = SimulatedTime.AddHours(1);
        CurrentTicketInfo = $"Zeit vorgestellt auf {SimulatedTime:HH:mm}.";
    }

    [RelayCommand]
    private async Task TakeTicketAsync()
    {
        var ticket = await _parkingService.AssignFreeSpaceAsync();
        if (ticket != null)
        {
            ticket.EntryTime = SimulatedTime;
            await _dbContext.SaveChangesAsync();

            CurrentTicketInfo = $"Ticket ID: {ticket.Id}\nStockwerk: {ticket.FloorNumber}, Platz: {ticket.SpaceNumber}\nEinfahrt: {ticket.EntryTime:HH:mm}";
            await OpenBarrierAsync(true);
            await LoadDataAsync();
        }
        else
        {
            CurrentTicketInfo = "Parkhaus ist voll!";
        }
    }

    [RelayCommand]
    private async Task EnterTenantAsync()
    {
        var tenant = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Code == TenantCodeInput && c.CustomerType == CustomerType.Tenant);
        if (tenant != null && tenant.IsActive)
        {
            var ticket = await _parkingService.AssignFreeSpaceAsync(tenant.Id);
            if (ticket != null)
            {
                ticket.EntryTime = SimulatedTime;
                await _dbContext.SaveChangesAsync();
                
                CurrentTicketInfo = $"Willkommen Dauermieter!\nStockwerk: {ticket.FloorNumber}, Platz: {ticket.SpaceNumber}";
                await OpenBarrierAsync(true);
                await LoadDataAsync();
            }
            else
            {
                CurrentTicketInfo = "Parkhaus ist voll!";
            }
        }
        else
        {
            CurrentTicketInfo = "Ungültiger oder inaktiver Dauermieter-Code.";
        }
        TenantCodeInput = string.Empty;
    }

    [RelayCommand]
    private async Task RegisterTenantAsync()
    {
        var freeSpace = await _dbContext.ParkingSpaces
            .Include(ps => ps.Floor)
            .FirstOrDefaultAsync(ps => ps.AssignedTenantId == null);

        if (freeSpace == null)
        {
            CurrentTicketInfo = "Keine freien Parkplätze für neue Dauermieter verfügbar.";
            return;
        }

        string generatedCode = new Random().Next(1000, 9999).ToString();
        var customer = new Customer { Code = generatedCode, CustomerType = CustomerType.Tenant, IsActive = true };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        freeSpace.AssignedTenantId = customer.Id;
        await _dbContext.SaveChangesAsync();

        CurrentTicketInfo = $"Neuer Dauermieter erfolgreich registriert!\nPersönlicher Code: {customer.Code}\nZugewiesener Platz: Stockwerk {freeSpace.Floor?.Number}, Platz {freeSpace.Number}";
        await LoadDataAsync();
    }

    partial void OnSelectedTicketToPayChanged(Ticket? value)
    {
        if (value != null)
        {
            Task.Run(async () =>
            {
                var cost = await _tariffService.CalculateCostAsync(value.EntryTime, SimulatedTime);
                var duration = SimulatedTime - value.EntryTime;
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTicketInfo = $"Ticket {value.Id} ausgewählt.\nGelöst am: {value.EntryTime:dd.MM.yyyy HH:mm}\nVergangene Zeit: {duration.Hours}h {duration.Minutes}m\nZu bezahlen: CHF {cost:F2}";
                });
            });
        }
    }

    [RelayCommand]
    private async Task PayTicketAsync()
    {
        if (SelectedTicketToPay != null)
        {
            var cost = await _tariffService.CalculateCostAsync(SelectedTicketToPay.EntryTime, SimulatedTime);
            SelectedTicketToPay.Cost = cost;
            SelectedTicketToPay.IsPaid = true;
            _paymentTimes[SelectedTicketToPay.Id] = SimulatedTime;
            
            await _dbContext.SaveChangesAsync();
            CurrentTicketInfo = $"Ticket {SelectedTicketToPay.Id} bezahlt.\nAustrittsticket generiert!\nDatum: {SimulatedTime:dd.MM.yyyy HH:mm}\nBetrag: CHF {cost:F2}";
            SelectedTicketToPay = null;
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ExitOccasionalAsync()
    {
        if (SelectedTicketToExit != null)
        {
            if (SelectedTicketToExit.CustomerId != null)
            {
                CurrentTicketInfo = "Dieses Ticket gehört einem Dauermieter. Bitte Dauermieter-Ausfahrt nutzen.";
                return;
            }

            if (!SelectedTicketToExit.IsPaid)
            {
                CurrentTicketInfo = $"Ticket {SelectedTicketToExit.Id} ist nicht bezahlt!";
                return;
            }

            if (_paymentTimes.TryGetValue(SelectedTicketToExit.Id, out var payTime))
            {
                if ((SimulatedTime - payTime).TotalMinutes > 15)
                {
                    CurrentTicketInfo = $"15 Minuten seit Bezahlung überschritten. Bitte erneut bezahlen.";
                    SelectedTicketToExit.IsPaid = false;
                    await _dbContext.SaveChangesAsync();
                    await LoadDataAsync();
                    return;
                }
            }

            bool success = await _parkingService.FreeSpaceAsync(SelectedTicketToExit.Id, SimulatedTime);
            if (success)
            {
                CurrentTicketInfo = $"Auf Wiedersehen! Ticket {SelectedTicketToExit.Id} hat das Parkhaus verlassen.";
                SelectedTicketToExit = null;
                await OpenBarrierAsync(false);
                await LoadDataAsync();
            }
        }
    }

    [RelayCommand]
    private async Task ExitTenantAsync()
    {
        var tenant = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Code == TenantCodeInput && c.CustomerType == CustomerType.Tenant);
        if (tenant != null)
        {
            var activeTicket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.CustomerId == tenant.Id && t.ExitTime == null);
            if (activeTicket != null)
            {
                bool success = await _parkingService.FreeSpaceAsync(activeTicket.Id, SimulatedTime);
                if (success)
                {
                    CurrentTicketInfo = $"Auf Wiedersehen Dauermieter!";
                    await OpenBarrierAsync(false);
                    await LoadDataAsync();
                }
            }
            else
            {
                CurrentTicketInfo = "Kein aktiver Parkvorgang für diesen Dauermieter gefunden.";
            }
        }
        else
        {
            CurrentTicketInfo = "Ungültiger Dauermieter-Code.";
        }
        TenantCodeInput = string.Empty;
    }

    private async Task OpenBarrierAsync(bool isEntrance)
    {
        if (isEntrance) EntranceBarrierOpen = true;
        else ExitBarrierOpen = true;

        await Task.Delay(3000);

        if (isEntrance) EntranceBarrierOpen = false;
        else ExitBarrierOpen = false;
    }
}
