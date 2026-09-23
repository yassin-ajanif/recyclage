using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class SuppliersViewModel : EditableGridViewModelBase<SupplierRowViewModel>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "الموردون";

    public ObservableCollection<SupplierRowViewModel> Rows { get; } = [];

    protected override ObservableCollection<SupplierRowViewModel> EditableRows => Rows;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public SuppliersViewModel(IDbContextFactory<AppDbContext> dbFactory)
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
            var suppliers = await db.Suppliers
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            Rows.Clear();
            foreach (var supplier in suppliers)
                Rows.Add(ToRow(supplier));

            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل الموردين: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task<bool> SaveRowAsync(SupplierRowViewModel row)
    {
        if (row.IsEmpty || IsBusy)
            return false;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (row.Id == 0)
            {
                var entity = new Supplier
                {
                    Name = row.Name.Trim(),
                    Phone = row.Phone.Trim(),
                    Ice = row.Ice.Trim()
                };

                db.Suppliers.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == row.Id);
                if (entity is null)
                    return false;

                entity.Name = row.Name.Trim();
                entity.Phone = row.Phone.Trim();
                entity.Ice = row.Ice.Trim();
                await db.SaveChangesAsync();
                row.EndEdit();
            }

            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ المورد: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRow(SupplierRowViewModel row)
    {
        if (row.Id == 0)
            return;

        if (!await ConfirmDialogService.ConfirmDeleteAsync(row.Name))
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == row.Id);
            if (entity is not null)
            {
                db.Suppliers.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حذف المورد: {ex.Message}";
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
            var row = new SupplierRowViewModel();
            row.StartAsNewRow();
            Rows.Add(row);
        }
    }

    private static SupplierRowViewModel ToRow(Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Phone = supplier.Phone,
        Ice = supplier.Ice
    };
}
