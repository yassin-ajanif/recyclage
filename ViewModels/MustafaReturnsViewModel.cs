using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class MustafaReturnsViewModel : EditableGridViewModelBase<MustafaReturnEntryRowViewModel>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "رجوع لمصطفى";

    public ObservableCollection<MustafaReturnEntryRowViewModel> Rows { get; } = [];

    protected override ObservableCollection<MustafaReturnEntryRowViewModel> EditableRows => Rows;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private decimal _totalRemaining;

    public MustafaReturnsViewModel(IDbContextFactory<AppDbContext> dbFactory)
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
            var returns = await LoadReturnsAsync();

            Rows.Clear();
            foreach (var item in returns)
                Rows.Add(ToRow(item));

            RecalculateTotal();
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل رجوع لمصطفى: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task<bool> SaveRowAsync(MustafaReturnEntryRowViewModel row)
    {
        if (row.IsEmpty || IsBusy)
            return false;

        if (string.IsNullOrWhiteSpace(row.Date))
        {
            StatusMessage = "التاريخ مطلوب.";
            return false;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (row.Id == 0)
            {
                var entity = new MustafaReturn
                {
                    Date = row.Date.Trim(),
                    Amount = row.Amount,
                    Details = row.Details.Trim()
                };

                db.MustafaReturns.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.MustafaReturns.FirstOrDefaultAsync(r => r.Id == row.Id);
                if (entity is null)
                    return false;

                entity.Date = row.Date.Trim();
                entity.Amount = row.Amount;
                entity.Details = row.Details.Trim();
                await db.SaveChangesAsync();
                row.EndEdit();
            }

            await ReloadAndRecalculateAsync();
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

    [RelayCommand]
    private async Task DeleteRow(MustafaReturnEntryRowViewModel row)
    {
        if (row.Id == 0)
            return;

        if (!await ConfirmDialogService.ConfirmDeleteAsync(FormatRowLabel(row)))
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.MustafaReturns.FirstOrDefaultAsync(r => r.Id == row.Id);
            if (entity is not null)
            {
                db.MustafaReturns.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            RecalculateTotal();
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

    private async Task<List<MustafaReturn>> LoadReturnsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.MustafaReturns
            .AsNoTracking()
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Id)
            .ToListAsync();
    }

    private async Task ReloadAndRecalculateAsync()
    {
        var returns = await LoadReturnsAsync();

        var editingNewRow = Rows.Count > 0 && Rows[^1].Id == 0 && Rows[^1].IsEditing;
        var newRow = editingNewRow ? Rows[^1] : null;

        Rows.Clear();
        foreach (var item in returns)
            Rows.Add(ToRow(item));

        if (newRow is not null)
            Rows.Add(newRow);
        else
            EnsureTrailingEmptyRow();

        RecalculateTotal();
    }

    private void RecalculateTotal() =>
        TotalRemaining = Rows.Where(r => r.Id > 0).Sum(r => r.Amount);

    private void EnsureTrailingEmptyRow()
    {
        if (Rows.Count == 0 || !Rows[^1].IsEmpty)
        {
            var row = new MustafaReturnEntryRowViewModel();
            row.StartAsNewRow();
            Rows.Add(row);
        }
        else if (!Rows[^1].IsEditing)
        {
            Rows[^1].StartAsNewRow();
        }
    }

    private static string? FormatRowLabel(MustafaReturnEntryRowViewModel row) =>
        row.Amount > 0 ? $"{row.Date} — {row.Amount:0.00} د.م" : row.Date;

    private static MustafaReturnEntryRowViewModel ToRow(MustafaReturn item) => new()
    {
        Id = item.Id,
        Date = item.Date,
        Amount = item.Amount,
        Details = item.Details
    };
}
