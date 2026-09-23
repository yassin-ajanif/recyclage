using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class CustomerInvoiceReportViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private Dictionary<string, int> _saleProductIdsByName = [];

    public override string Title => "فاتورة زبون";

    public ObservableCollection<NamedOption> Clients { get; } = [];
    public ObservableCollection<string> SaleProductNames { get; } = [];
    public ObservableCollection<SaleEntryRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    private NamedOption? _selectedClient;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasSaleProducts => SaleProductNames.Count > 0;
    public bool ShowSaleProductsHint => SaleProductNames.Count == 0;

    public CustomerInvoiceReportViewModel(IDbContextFactory<AppDbContext> dbFactory)
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
            SelectedClient = Clients.FirstOrDefault();
            await LoadRowsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedClientChanged(NamedOption? value) => _ = LoadRowsAsync();

    private async Task LoadLookupsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var clients = await db.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        Clients.Clear();
        foreach (var c in clients)
            Clients.Add(new NamedOption { Id = c.Id, Name = c.Name });

        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.ProductType == ProductType.ForSale)
            .OrderBy(p => p.Name)
            .ToListAsync();

        _saleProductIdsByName = products.ToDictionary(p => p.Name, p => p.Id);
        SaleProductNames.Clear();
        foreach (var name in _saleProductIdsByName.Keys.OrderBy(n => n))
            SaleProductNames.Add(name);

        OnPropertyChanged(nameof(HasSaleProducts));
        OnPropertyChanged(nameof(ShowSaleProductsHint));
    }

    private async Task LoadRowsAsync()
    {
        Rows.Clear();
        if (SelectedClient is null)
        {
            EnsureTrailingEmptyRow();
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var sales = await db.Sales
            .AsNoTracking()
            .Include(s => s.Product)
            .Where(s => s.ClientId == SelectedClient.Id)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.Id)
            .ToListAsync();

        foreach (var sale in sales)
            Rows.Add(ToRow(sale));

        EnsureTrailingEmptyRow();
    }

    public async Task SaveRowAsync(SaleEntryRowViewModel row)
    {
        if (row.IsEmpty || SelectedClient is null || IsBusy)
            return;

        if (!TryResolveProduct(row, out var productId))
            return;

        row.ProductId = productId;
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
                var entity = new Sale
                {
                    Date = row.Date.Trim(),
                    Quantity = row.Quantity,
                    ProductId = row.ProductId,
                    ClientId = SelectedClient.Id,
                    UnitPrice = row.UnitPrice,
                    TransportCost = row.TransportCost,
                    Total = row.Total,
                    Paid = row.Paid,
                    Remaining = row.Remaining
                };

                db.Sales.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.Sales.FirstOrDefaultAsync(s => s.Id == row.Id);
                if (entity is null)
                    return;

                entity.Date = row.Date.Trim();
                entity.Quantity = row.Quantity;
                entity.ProductId = row.ProductId;
                entity.ClientId = SelectedClient.Id;
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

    public void ApplyProductSelection(SaleEntryRowViewModel row, string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return;

        row.ProductName = productName.Trim();
        if (TryResolveProduct(row, out var productId))
            row.ProductId = productId;
    }

    private bool TryResolveProduct(SaleEntryRowViewModel row, out int productId)
    {
        productId = 0;
        if (string.IsNullOrWhiteSpace(row.ProductName))
        {
            StatusMessage = "اختر منتجاً قبل الحفظ.";
            return false;
        }

        var name = row.ProductName.Trim();
        if (_saleProductIdsByName.TryGetValue(name, out productId))
            return true;

        StatusMessage = "المنتج غير موجود أو ليس من نوع «للبيع».";
        return false;
    }

    [RelayCommand]
    private async Task DeleteRow(SaleEntryRowViewModel row)
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
            var entity = await db.Sales.FirstOrDefaultAsync(s => s.Id == row.Id);
            if (entity is not null)
            {
                db.Sales.Remove(entity);
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
        {
            var row = new SaleEntryRowViewModel();
            row.AttachProductNames(SaleProductNames);
            Rows.Add(row);
        }
    }

    private SaleEntryRowViewModel ToRow(Sale sale)
    {
        var name = sale.Product.Name;
        if (!_saleProductIdsByName.ContainsKey(name))
        {
            _saleProductIdsByName[name] = sale.ProductId;
            if (!SaleProductNames.Contains(name))
                SaleProductNames.Add(name);
        }

        var row = new SaleEntryRowViewModel
        {
            Id = sale.Id,
            Date = sale.Date,
            Quantity = sale.Quantity,
            ProductName = name,
            ProductId = sale.ProductId,
            UnitPrice = sale.UnitPrice,
            TransportCost = sale.TransportCost,
            Paid = sale.Paid,
            Total = sale.Total,
            Remaining = sale.Remaining
        };
        row.AttachProductNames(SaleProductNames);
        return row;
    }
}
