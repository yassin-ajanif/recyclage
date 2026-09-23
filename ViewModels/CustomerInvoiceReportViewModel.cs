using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Demo;
using Recyclage.Models;
using Recyclage.Shared.Database;

namespace Recyclage.ViewModels;

public partial class CustomerInvoiceReportViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة زبون";

    public ObservableCollection<string> Clients { get; } = [];

    [ObservableProperty]
    private string? _selectedClient;

    public ObservableCollection<SaleRow> Rows { get; } = [];

    public CustomerInvoiceReportViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var names = await db.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();

        Clients.Clear();
        foreach (var name in names)
            Clients.Add(name);

        SelectedClient = Clients.FirstOrDefault();
        RefreshRows();
    }

    partial void OnSelectedClientChanged(string? value) => RefreshRows();

    private void RefreshRows()
    {
        Rows.Clear();
        if (string.IsNullOrWhiteSpace(SelectedClient))
            return;

        foreach (var row in DemoData.Sales.Where(r => r.Client == SelectedClient))
            Rows.Add(row);
    }
}
