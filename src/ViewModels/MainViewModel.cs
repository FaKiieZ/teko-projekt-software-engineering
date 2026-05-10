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

    private DateTime _simulatedTime;
    public DateTime SimulatedTime
    {
        get => _simulatedTime;
        set
        {
            if (SetProperty(ref _simulatedTime, value))
            {
                OnPropertyChanged(nameof(SimulatedDate));
                OnPropertyChanged(nameof(SimulatedTimeOnly));
                TakeTicketCommand.NotifyCanExecuteChanged();
                EnterTenantCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public DateTime? SimulatedDate
    {
        get => SimulatedTime;
        set
        {
            if (value.HasValue)
            {
                SimulatedTime = value.Value.Date + SimulatedTime.TimeOfDay;
            }
        }
    }

    public DateTime? SimulatedTimeOnly
    {
        get => SimulatedTime;
        set
        {
            if (value.HasValue)
            {
                SimulatedTime = SimulatedTime.Date + value.Value.TimeOfDay;
            }
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TakeTicketCommand))]
    [NotifyCanExecuteChangedFor(nameof(EnterTenantCommand))]
    private bool _entranceBarrierOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExitOccasionalCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExitTenantCommand))]
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
    private int _selectedModuleIndex = 0; // 0: Dashboard, 1: Verwaltung, 2: Reporting

    [ObservableProperty]
    private bool _isSidebarCollapsed = false;

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
    }

    [RelayCommand]
    private void SelectModule(string index)
    {
        if (int.TryParse(index, out int idx))
        {
            SelectedModuleIndex = idx;
        }
    }

    [ObservableProperty]
    private decimal _totalRevenue;

    private bool CanEnter() => !EntranceBarrierOpen;
    private bool CanExit() => !ExitBarrierOpen;

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
        await _dbContext.Database.EnsureDeletedAsync(); // Refresh für Prototyp, um eine saubere Ausgangslage zu haben
        await _dbContext.Database.EnsureCreatedAsync();
        
        if (!await _dbContext.Floors.AnyAsync())
        {
            var garage = new ParkingGarage { Name = "Teko Garage" };
            _dbContext.ParkingGarages.Add(garage);
            await _dbContext.SaveChangesAsync();

            int[] floorSizes = [15, 10, 5];
            for (int i = 1; i <= 3; i++)
            {
                var floor = new Floor { Number = i, TotalSpaces = floorSizes[i - 1], ParkingGarageId = garage.Id };
                for (int j = 1; j <= floorSizes[i - 1]; j++)
                {
                    floor.ParkingSpaces.Add(new ParkingSpace { Number = j });
                }
                _dbContext.Floors.Add(floor);
            }

            // Einen Dauermieter anlegen (Seeding)
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
    private void AdvanceTime(string minutes)
    {
        if (int.TryParse(minutes, out int mins))
        {
            SimulatedTime = SimulatedTime.AddMinutes(mins);
            CurrentTicketInfo = $"Zeit vorgestellt um {mins} Minuten auf {SimulatedTime:HH:mm}.";
        }
    }

    [RelayCommand]
    private void ResetTime()
    {
        SimulatedTime = DateTime.Now;
        CurrentTicketInfo = "Zeit auf aktuelle Systemzeit zurückgesetzt.";
    }

    [RelayCommand(CanExecute = nameof(CanEnter))]
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

    [RelayCommand(CanExecute = nameof(CanEnter))]
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

    [RelayCommand(CanExecute = nameof(CanExit))]
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

    [RelayCommand(CanExecute = nameof(CanExit))]
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

    [RelayCommand]
    private async Task SelectParkingSpaceAsync(ParkingSpace space)
    {
        if (space == null) return;

        var floor = Floors.FirstOrDefault(f => f.Id == space.FloorId);
        if (floor == null) return;

        if (space.IsOccupied)
        {
            var ticket = await _dbContext.Tickets
                .Include(t => t.Customer)
                .Where(t => t.FloorNumber == floor.Number && t.SpaceNumber == space.Number && t.ExitTime == null)
                .OrderByDescending(t => t.EntryTime)
                .FirstOrDefaultAsync();

            if (ticket != null)
            {
                var duration = SimulatedTime - ticket.EntryTime;
                string type = ticket.CustomerId != null ? "Dauermieter" : "Gelegenheitsnutzer";
                string paidStatus = ticket.IsPaid ? "Bezahlt" : "Offen";
                
                CurrentTicketInfo = $"Parkplatz {floor.Number}.{space.Number} ({type})\n" +
                                    $"Ticket ID: {ticket.Id}\n" +
                                    $"Einfahrt: {ticket.EntryTime:dd.MM.yyyy HH:mm}\n" +
                                    $"Dauer: {duration.Days}d {duration.Hours}h {duration.Minutes}m\n" +
                                    $"Status: {paidStatus}";
                
                if (ticket.CustomerId == null && !ticket.IsPaid)
                {
                    var cost = await _tariffService.CalculateCostAsync(ticket.EntryTime, SimulatedTime);
                    CurrentTicketInfo += $"\nAktuelle Kosten: CHF {cost:F2}";
                }
            }
            else
            {
                CurrentTicketInfo = $"Parkplatz {floor.Number}.{space.Number} ist besetzt, aber kein Ticket gefunden.";
            }
        }
        else
        {
            string status = space.AssignedTenantId != null ? $"Reserviert für Dauermieter (ID: {space.AssignedTenantId})" : "Frei";
            CurrentTicketInfo = $"Parkplatz {floor.Number}.{space.Number}\nStatus: {status}";
        }
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
