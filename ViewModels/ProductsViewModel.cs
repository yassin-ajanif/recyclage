using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;

namespace Recyclage.ViewModels;

public partial class ProductsViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "المنتجات";

    public static IReadOnlyList<string> ProductTypeLabels { get; } = ["للشراء", "للبيع"];

    public ObservableCollection<ProductRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ProductsViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var products = await db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ThenBy(p => p.ProductType)
                .ToListAsync();

            Rows.Clear();
            foreach (var product in products)
                Rows.Add(ToRow(product));

            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل المنتجات: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveRowAsync(ProductRowViewModel row)
    {
        if (row.IsEmpty)
            return;

        if (row.DefaultUnitPrice < 0)
        {
            StatusMessage = "الثمن لا يمكن أن يكون سالباً.";
            return;
        }

        var productType = ParseProductType(row.ProductType);
        if (productType is null)
        {
            StatusMessage = "نوع المنتج: للشراء أو للبيع فقط.";
            return;
        }

        if (IsBusy)
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (row.Id == 0)
            {
                var entity = new Product
                {
                    Name = row.Name.Trim(),
                    ProductType = productType.Value,
                    DefaultUnit = string.IsNullOrWhiteSpace(row.DefaultUnit) ? "كغ" : row.DefaultUnit.Trim(),
                    DefaultUnitPrice = row.DefaultUnitPrice
                };

                db.Products.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == row.Id);
                if (entity is null)
                    return;

                entity.Name = row.Name.Trim();
                entity.ProductType = productType.Value;
                entity.DefaultUnit = string.IsNullOrWhiteSpace(row.DefaultUnit) ? "كغ" : row.DefaultUnit.Trim();
                entity.DefaultUnitPrice = row.DefaultUnitPrice;
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ المنتج: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRow(ProductRowViewModel row)
    {
        if (row.Id == 0)
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == row.Id);
            if (entity is not null)
            {
                db.Products.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حذف المنتج: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void EnsureTrailingEmptyRow()
    {
        if (Rows.Count == 0 || !Rows[^1].IsEmpty)
            Rows.Add(new ProductRowViewModel());
    }

    private static ProductRowViewModel ToRow(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        ProductType = ProductTypeDisplay.ToArabic(product.ProductType),
        DefaultUnit = product.DefaultUnit,
        DefaultUnitPrice = product.DefaultUnitPrice
    };

    private static ProductType? ParseProductType(string label) => label.Trim() switch
    {
        "للشراء" => ProductType.ForBuying,
        "للبيع" => ProductType.ForSale,
        _ => null
    };
}
