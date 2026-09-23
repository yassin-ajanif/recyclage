using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Models;
using Recyclage.Shared.Database;

namespace Recyclage.ViewModels;

public partial class PurchaseInvoicesViewModel : MonthFilteredPageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة المشتريات";

    public ObservableCollection<PurchaseInvoiceRow> Rows { get; } = [];
    public ObservableCollection<NamedOption> Suppliers { get; } = [];

    [ObservableProperty]
    private NamedOption? _selectedSupplier;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private decimal _totalPaid;

    [ObservableProperty]
    private decimal _totalRemaining;

    public PurchaseInvoicesViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = InitializeAsync();
    }

    protected override void OnMonthFilterChanged() => _ = LoadAsync();

    partial void OnSelectedSupplierChanged(NamedOption? value) => _ = LoadAsync();

    private async Task InitializeAsync()
    {
        await LoadSuppliersAsync();
        SelectedSupplier = Suppliers.FirstOrDefault();
    }

    private async Task LoadSuppliersAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var suppliers = await db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync();

        Suppliers.Clear();
        Suppliers.Add(new NamedOption { Id = 0, Name = "الكل" });
        foreach (var supplier in suppliers)
            Suppliers.Add(new NamedOption { Id = supplier.Id, Name = supplier.Name });
    }

    private async Task LoadAsync()
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var query = db.PurchaseInvoices
                .AsNoTracking()
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .Where(p => p.Date.StartsWith(SelectedMonthPrefix));

            if (SelectedSupplier is { Id: > 0 })
                query = query.Where(p => p.SupplierId == SelectedSupplier.Id);

            var invoices = await query
                .OrderByDescending(p => p.Date)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            Rows.Clear();
            foreach (var invoice in invoices)
            {
                Rows.Add(new PurchaseInvoiceRow
                {
                    Date = invoice.Date,
                    Quantity = invoice.Quantity,
                    Product = invoice.Product.Name,
                    Supplier = invoice.Supplier.Name,
                    UnitPrice = invoice.UnitPrice.ToString("N2"),
                    TransportCost = invoice.TransportCost.ToString("N2"),
                    Total = invoice.Total.ToString("N2"),
                    Paid = invoice.Paid.ToString("N2"),
                    Remaining = invoice.Remaining.ToString("N2")
                });
            }

            GrandTotal = invoices.Sum(i => i.Total);
            TotalPaid = invoices.Sum(i => i.Paid);
            TotalRemaining = invoices.Sum(i => i.Remaining);
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل المشتريات: {ex.Message}";
        }
    }
}
