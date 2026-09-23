using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;

namespace Recyclage.ViewModels;

public partial class SupplierInvoiceReportViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة مورد";

    public ObservableCollection<NamedOption> Suppliers { get; } = [];
    public ObservableCollection<NamedOption> BuyingProducts { get; } = [];
    public ObservableCollection<PurchaseInvoiceEntryRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    private NamedOption? _selectedSupplier;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public SupplierInvoiceReportViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            await LoadLookupsAsync();
            SelectedSupplier = Suppliers.FirstOrDefault();
            await LoadRowsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedSupplierChanged(NamedOption? value) => _ = LoadRowsAsync();

    private async Task LoadLookupsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var suppliers = await db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        Suppliers.Clear();
        foreach (var s in suppliers)
            Suppliers.Add(new NamedOption { Id = s.Id, Name = s.Name });

        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.ProductType == ProductType.ForBuying)
            .OrderBy(p => p.Name)
            .ToListAsync();

        BuyingProducts.Clear();
        foreach (var p in products)
            BuyingProducts.Add(new NamedOption { Id = p.Id, Name = p.Name });
    }

    private async Task LoadRowsAsync()
    {
        Rows.Clear();
        if (SelectedSupplier is null)
        {
            EnsureTrailingEmptyRow();
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var invoices = await db.PurchaseInvoices
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.SupplierId == SelectedSupplier.Id)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        foreach (var invoice in invoices)
            Rows.Add(ToRow(invoice, BuyingProducts.FirstOrDefault(p => p.Id == invoice.ProductId)));

        EnsureTrailingEmptyRow();
    }

    public async Task SaveRowAsync(PurchaseInvoiceEntryRowViewModel row)
    {
        if (row.IsEmpty || SelectedSupplier is null || IsBusy)
            return;

        row.RecalculateTotals();
        if (row.Quantity <= 0)
        {
            StatusMessage = "الكمية يجب أن تكون أكبر من صفر.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (row.Id == 0)
            {
                var entity = new PurchaseInvoice
                {
                    Date = row.Date.Trim(),
                    Quantity = row.Quantity,
                    ProductId = row.ProductId,
                    SupplierId = SelectedSupplier.Id,
                    UnitPrice = row.UnitPrice,
                    TransportCost = row.TransportCost,
                    Total = row.Total,
                    Paid = row.Paid,
                    Remaining = row.Remaining
                };

                db.PurchaseInvoices.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.PurchaseInvoices.FirstOrDefaultAsync(p => p.Id == row.Id);
                if (entity is null)
                    return;

                entity.Date = row.Date.Trim();
                entity.Quantity = row.Quantity;
                entity.ProductId = row.ProductId;
                entity.SupplierId = SelectedSupplier.Id;
                entity.UnitPrice = row.UnitPrice;
                entity.TransportCost = row.TransportCost;
                entity.Total = row.Total;
                entity.Paid = row.Paid;
                entity.Remaining = row.Remaining;
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ السطر: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ApplyProductSelection(PurchaseInvoiceEntryRowViewModel row, NamedOption? product)
    {
        row.SelectedProduct = product;
        row.ProductId = product?.Id ?? 0;
    }

    [RelayCommand]
    private async Task DeleteRow(PurchaseInvoiceEntryRowViewModel row)
    {
        if (row.Id == 0)
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.PurchaseInvoices.FirstOrDefaultAsync(p => p.Id == row.Id);
            if (entity is not null)
            {
                db.PurchaseInvoices.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حذف السطر: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void EnsureTrailingEmptyRow()
    {
        if (Rows.Count == 0 || !Rows[^1].IsEmpty)
            Rows.Add(new PurchaseInvoiceEntryRowViewModel());
    }

    private static PurchaseInvoiceEntryRowViewModel ToRow(PurchaseInvoice invoice, NamedOption? product)
    {
        var row = new PurchaseInvoiceEntryRowViewModel
        {
            Id = invoice.Id,
            Date = invoice.Date,
            Quantity = invoice.Quantity,
            ProductId = invoice.ProductId,
            UnitPrice = invoice.UnitPrice,
            TransportCost = invoice.TransportCost,
            Paid = invoice.Paid,
            Total = invoice.Total,
            Remaining = invoice.Remaining,
            SelectedProduct = product ?? new NamedOption { Id = invoice.ProductId, Name = invoice.Product.Name }
        };
        return row;
    }
}
