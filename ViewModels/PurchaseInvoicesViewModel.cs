using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Models;
using Recyclage.Shared.Database;

namespace Recyclage.ViewModels;

public partial class PurchaseInvoicesViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة المشتريات";

    public ObservableCollection<PurchaseInvoiceRow> Rows { get; } = [];

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
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var invoices = await db.PurchaseInvoices
                .AsNoTracking()
                .Include(p => p.Product)
                .Include(p => p.Supplier)
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
