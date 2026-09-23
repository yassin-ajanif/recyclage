using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class SupplierInvoiceReportViewModel : MonthFilteredEditableGridViewModelBase<PurchaseInvoiceEntryRowViewModel>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private Dictionary<string, int> _buyingProductIdsByName = [];

    public override string Title => "فاتورة مورد";

    public ObservableCollection<NamedOption> Suppliers { get; } = [];
    public ObservableCollection<string> BuyingProductNames { get; } = [];
    public ObservableCollection<PurchaseInvoiceEntryRowViewModel> Rows { get; } = [];

    protected override ObservableCollection<PurchaseInvoiceEntryRowViewModel> EditableRows => Rows;

    [ObservableProperty]
    private NamedOption? _selectedSupplier;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private decimal _totalPaid;

    [ObservableProperty]
    private decimal _totalRemaining;

    public bool HasBuyingProducts => BuyingProductNames.Count > 0;
    public bool ShowBuyingProductsHint => BuyingProductNames.Count == 0;

    public SupplierInvoiceReportViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        Rows.CollectionChanged += OnRowsCollectionChanged;
        _ = InitializeAsync();
    }

    private void OnRowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        RecalculateFooterTotals();

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

    protected override void OnMonthFilterChanged() => _ = LoadRowsAsync();

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

        _buyingProductIdsByName = products.ToDictionary(p => p.Name, p => p.Id);
        BuyingProductNames.Clear();
        foreach (var name in _buyingProductIdsByName.Keys.OrderBy(n => n))
            BuyingProductNames.Add(name);

        OnPropertyChanged(nameof(HasBuyingProducts));
        OnPropertyChanged(nameof(ShowBuyingProductsHint));
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
            .Where(p => p.SupplierId == SelectedSupplier.Id && p.Date.StartsWith(SelectedMonthPrefix))
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        foreach (var invoice in invoices)
            Rows.Add(ToRow(invoice));

        EnsureTrailingEmptyRow();
        RecalculateFooterTotals();
    }

    public override async Task<bool> SaveRowAsync(PurchaseInvoiceEntryRowViewModel row)
    {
        if (row.IsEmpty || SelectedSupplier is null || IsBusy)
            return false;

        if (!TryResolveProduct(row, out var productId))
            return false;

        row.ProductId = productId;
        row.RecalculateTotals();

        if (row.Quantity <= 0)
        {
            StatusMessage = "الكمية يجب أن تكون أكبر من صفر.";
            return false;
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
                    return false;

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
                row.EndEdit();
            }

            RecalculateFooterTotals();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ السطر: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ApplyProductSelection(PurchaseInvoiceEntryRowViewModel row, string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return;

        row.ProductName = productName.Trim();
        if (TryResolveProduct(row, out var productId))
            row.ProductId = productId;
    }

    private bool TryResolveProduct(PurchaseInvoiceEntryRowViewModel row, out int productId)
    {
        productId = 0;
        if (string.IsNullOrWhiteSpace(row.ProductName))
        {
            StatusMessage = "اختر منتجاً قبل الحفظ.";
            return false;
        }

        var name = row.ProductName.Trim();
        if (_buyingProductIdsByName.TryGetValue(name, out productId))
            return true;

        StatusMessage = "المنتج غير موجود أو ليس من نوع «للشراء».";
        return false;
    }

    [RelayCommand]
    private async Task DeleteRow(PurchaseInvoiceEntryRowViewModel row)
    {
        if (row.Id == 0)
            return;

        var label = string.IsNullOrWhiteSpace(row.ProductName) ? null : row.ProductName;
        if (!await ConfirmDialogService.ConfirmDeleteAsync(label))
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
            RecalculateFooterTotals();
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

    private void RecalculateFooterTotals()
    {
        var savedRows = Rows.Where(r => r.Id > 0);
        GrandTotal = savedRows.Sum(r => r.Total);
        TotalPaid = savedRows.Sum(r => r.Paid);
        TotalRemaining = savedRows.Sum(r => r.Remaining);
    }

    private void EnsureTrailingEmptyRow()
    {
        if (Rows.Count == 0 || !Rows[^1].IsEmpty)
        {
            var row = new PurchaseInvoiceEntryRowViewModel();
            row.AttachProductNames(BuyingProductNames);
            row.StartAsNewRow();
            Rows.Add(row);
        }
        else if (!Rows[^1].IsEditing)
        {
            Rows[^1].StartAsNewRow();
        }
    }

    private PurchaseInvoiceEntryRowViewModel ToRow(PurchaseInvoice invoice)
    {
        var name = invoice.Product.Name;
        if (!_buyingProductIdsByName.ContainsKey(name))
        {
            _buyingProductIdsByName[name] = invoice.ProductId;
            if (!BuyingProductNames.Contains(name))
                BuyingProductNames.Add(name);
        }

        var row = new PurchaseInvoiceEntryRowViewModel
        {
            Id = invoice.Id,
            Date = invoice.Date,
            Quantity = invoice.Quantity,
            ProductName = name,
            ProductId = invoice.ProductId,
            UnitPrice = invoice.UnitPrice,
            TransportCost = invoice.TransportCost,
            Paid = invoice.Paid,
            Total = invoice.Total,
            Remaining = invoice.Remaining
        };
        row.AttachProductNames(BuyingProductNames);
        return row;
    }
}
