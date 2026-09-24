using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class PartnerTransactionsViewModel : EditableGridViewModelBase<PartnerTransactionEntryRowViewModel>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "حساب بين الشركاء";

    public ObservableCollection<PartnerTransactionEntryRowViewModel> Rows { get; } = [];

    protected override ObservableCollection<PartnerTransactionEntryRowViewModel> EditableRows => Rows;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public PartnerTransactionsViewModel(IDbContextFactory<AppDbContext> dbFactory)
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
            var transactions = await db.PartnerTransactions
                .AsNoTracking()
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            Rows.Clear();
            foreach (var transaction in transactions)
                Rows.Add(ToRow(transaction));

            PartnerBalanceCalculator.ApplyRunningBalances(Rows);
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل حساب الشركاء: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task<bool> SaveRowAsync(PartnerTransactionEntryRowViewModel row)
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
                var entity = new PartnerTransaction
                {
                    Date = row.Date.Trim(),
                    PaidByAyoub = row.PaidByAyoub,
                    ReturnedToMustafa = row.ReturnedToMustafa,
                    Details = row.Details.Trim()
                };

                db.PartnerTransactions.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.PartnerTransactions.FirstOrDefaultAsync(t => t.Id == row.Id);
                if (entity is null)
                    return false;

                entity.Date = row.Date.Trim();
                entity.PaidByAyoub = row.PaidByAyoub;
                entity.ReturnedToMustafa = row.ReturnedToMustafa;
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
    private async Task DeleteRow(PartnerTransactionEntryRowViewModel row)
    {
        if (row.Id == 0)
            return;

        if (!await ConfirmDialogService.ConfirmDeleteAsync(row.Details))
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.PartnerTransactions.FirstOrDefaultAsync(t => t.Id == row.Id);
            if (entity is not null)
            {
                db.PartnerTransactions.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            EnsureTrailingEmptyRow();
            PartnerBalanceCalculator.ApplyRunningBalances(Rows);
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

    private async Task ReloadAndRecalculateAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var transactions = await db.PartnerTransactions
            .AsNoTracking()
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        var editingNewRow = Rows.Count > 0 && Rows[^1].Id == 0 && Rows[^1].IsEditing;
        var newRow = editingNewRow ? Rows[^1] : null;

        Rows.Clear();
        foreach (var transaction in transactions)
            Rows.Add(ToRow(transaction));

        PartnerBalanceCalculator.ApplyRunningBalances(Rows);

        if (newRow is not null)
            Rows.Add(newRow);
        else
            EnsureTrailingEmptyRow();
    }

    private void EnsureTrailingEmptyRow()
    {
        if (Rows.Count == 0 || !Rows[^1].IsEmpty)
        {
            var row = new PartnerTransactionEntryRowViewModel();
            row.StartAsNewRow();
            Rows.Add(row);
        }
        else if (!Rows[^1].IsEditing)
        {
            Rows[^1].StartAsNewRow();
        }
    }

    private static PartnerTransactionEntryRowViewModel ToRow(PartnerTransaction transaction) => new()
    {
        Id = transaction.Id,
        Date = transaction.Date,
        PaidByAyoub = transaction.PaidByAyoub,
        ReturnedToMustafa = transaction.ReturnedToMustafa,
        Details = transaction.Details
    };
}
