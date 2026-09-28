using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class AyoubPaymentsViewModel : EditableGridViewModelBase<AyoubPaymentEntryRowViewModel>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "دفع لي أيوب";

    public ObservableCollection<AyoubPaymentEntryRowViewModel> Rows { get; } = [];

    protected override ObservableCollection<AyoubPaymentEntryRowViewModel> EditableRows => Rows;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private decimal _totalRemaining;

    public AyoubPaymentsViewModel(IDbContextFactory<AppDbContext> dbFactory)
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
            var payments = await LoadPaymentsAsync();

            Rows.Clear();
            foreach (var payment in payments)
                Rows.Add(ToRow(payment));

            RecalculateTotal();
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل دفع لي أيوب: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task<bool> SaveRowAsync(AyoubPaymentEntryRowViewModel row)
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
                var entity = new AyoubPayment
                {
                    Date = row.Date.Trim(),
                    Amount = row.Amount,
                    Details = row.Details.Trim()
                };

                db.AyoubPayments.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.AyoubPayments.FirstOrDefaultAsync(p => p.Id == row.Id);
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
    private async Task DeleteRow(AyoubPaymentEntryRowViewModel row)
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
            var entity = await db.AyoubPayments.FirstOrDefaultAsync(p => p.Id == row.Id);
            if (entity is not null)
            {
                db.AyoubPayments.Remove(entity);
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

    private async Task<List<AyoubPayment>> LoadPaymentsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AyoubPayments
            .AsNoTracking()
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .ToListAsync();
    }

    private async Task ReloadAndRecalculateAsync()
    {
        var payments = await LoadPaymentsAsync();

        var editingNewRow = Rows.Count > 0 && Rows[^1].Id == 0 && Rows[^1].IsEditing;
        var newRow = editingNewRow ? Rows[^1] : null;

        Rows.Clear();
        foreach (var payment in payments)
            Rows.Add(ToRow(payment));

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
            var row = new AyoubPaymentEntryRowViewModel();
            row.StartAsNewRow();
            Rows.Add(row);
        }
        else if (!Rows[^1].IsEditing)
        {
            Rows[^1].StartAsNewRow();
        }
    }

    private static string? FormatRowLabel(AyoubPaymentEntryRowViewModel row) =>
        row.Amount > 0 ? $"{row.Date} — {row.Amount:0.00} د.م" : row.Date;

    private static AyoubPaymentEntryRowViewModel ToRow(AyoubPayment payment) => new()
    {
        Id = payment.Id,
        Date = payment.Date,
        Amount = payment.Amount,
        Details = payment.Details
    };
}
