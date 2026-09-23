using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Models;
using Recyclage.Shared.Database;

namespace Recyclage.ViewModels;

public partial class SalesViewModel : MonthFilteredPageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة المبيعات";

    public ObservableCollection<SaleRow> Rows { get; } = [];
    public ObservableCollection<NamedOption> Clients { get; } = [];

    [ObservableProperty]
    private NamedOption? _selectedClient;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private decimal _totalPaid;

    [ObservableProperty]
    private decimal _totalRemaining;

    public SalesViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = InitializeAsync();
    }

    protected override void OnMonthFilterChanged() => _ = LoadAsync();

    partial void OnSelectedClientChanged(NamedOption? value) => _ = LoadAsync();

    private async Task InitializeAsync()
    {
        await LoadClientsAsync();
        SelectedClient = Clients.FirstOrDefault();
    }

    private async Task LoadClientsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var clients = await db.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

        Clients.Clear();
        Clients.Add(new NamedOption { Id = 0, Name = "الكل" });
        foreach (var client in clients)
            Clients.Add(new NamedOption { Id = client.Id, Name = client.Name });
    }

    private async Task LoadAsync()
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var query = db.Sales
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.Client)
                .Where(s => s.Date.StartsWith(SelectedMonthPrefix));

            if (SelectedClient is { Id: > 0 })
                query = query.Where(s => s.ClientId == SelectedClient.Id);

            var sales = await query
                .OrderByDescending(s => s.Date)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            Rows.Clear();
            foreach (var sale in sales)
            {
                Rows.Add(new SaleRow
                {
                    Date = sale.Date,
                    Quantity = sale.Quantity,
                    Product = sale.Product.Name,
                    Client = sale.Client.Name,
                    UnitPrice = sale.UnitPrice.ToString("N2"),
                    TransportCost = sale.TransportCost.ToString("N2"),
                    Total = sale.Total.ToString("N2"),
                    Paid = sale.Paid.ToString("N2"),
                    Remaining = sale.Remaining.ToString("N2")
                });
            }

            GrandTotal = sales.Sum(s => s.Total);
            TotalPaid = sales.Sum(s => s.Paid);
            TotalRemaining = sales.Sum(s => s.Remaining);
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل المبيعات: {ex.Message}";
        }
    }
}
