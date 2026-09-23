using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Demo;
using Recyclage.Models;
using Recyclage.Shared.Database;

namespace Recyclage.ViewModels;

public partial class SupplierInvoiceReportViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة مورد";

    public ObservableCollection<string> Suppliers { get; } = [];

    [ObservableProperty]
    private string? _selectedSupplier;

    public ObservableCollection<PurchaseInvoiceRow> Rows { get; } = [];

    public SupplierInvoiceReportViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = LoadSuppliersAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var names = await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => s.Name)
            .ToListAsync();

        Suppliers.Clear();
        foreach (var name in names)
            Suppliers.Add(name);

        SelectedSupplier = Suppliers.FirstOrDefault();
        RefreshRows();
    }

    partial void OnSelectedSupplierChanged(string? value) => RefreshRows();

    private void RefreshRows()
    {
        Rows.Clear();
        if (string.IsNullOrWhiteSpace(SelectedSupplier))
            return;

        foreach (var row in DemoData.PurchaseInvoices.Where(r => r.Supplier == SelectedSupplier))
            Rows.Add(row);
    }
}
