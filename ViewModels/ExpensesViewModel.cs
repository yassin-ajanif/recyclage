using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class ExpensesViewModel : EditableGridViewModelBase<ExpenseEntryRowViewModel>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "فاتورة المصاريف";

    public ObservableCollection<ExpenseEntryRowViewModel> Rows { get; } = [];

    protected override ObservableCollection<ExpenseEntryRowViewModel> EditableRows => Rows;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ExpensesViewModel(IDbContextFactory<AppDbContext> dbFactory)
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
            var expenses = await db.Expenses
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToListAsync();

            Rows.Clear();
            foreach (var expense in expenses)
                Rows.Add(ToRow(expense));

            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل المصاريف: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override async Task<bool> SaveRowAsync(ExpenseEntryRowViewModel row)
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
                var entity = new Expense
                {
                    Date = row.Date.Trim(),
                    ExpenseType = row.ExpenseType.Trim(),
                    Amount = row.Amount,
                    Description = row.Description.Trim()
                };

                db.Expenses.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.Expenses.FirstOrDefaultAsync(e => e.Id == row.Id);
                if (entity is null)
                    return false;

                entity.Date = row.Date.Trim();
                entity.ExpenseType = row.ExpenseType.Trim();
                entity.Amount = row.Amount;
                entity.Description = row.Description.Trim();
                await db.SaveChangesAsync();
                row.EndEdit();
            }

            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ المصروف: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRow(ExpenseEntryRowViewModel row)
    {
        if (row.Id == 0)
            return;

        if (!await ConfirmDialogService.ConfirmDeleteAsync(row.ExpenseType))
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.Expenses.FirstOrDefaultAsync(e => e.Id == row.Id);
            if (entity is not null)
            {
                db.Expenses.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حذف المصروف: {ex.Message}";
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
            var row = new ExpenseEntryRowViewModel();
            row.StartAsNewRow();
            Rows.Add(row);
        }
    }

    private static ExpenseEntryRowViewModel ToRow(Expense expense) => new()
    {
        Id = expense.Id,
        Date = expense.Date,
        ExpenseType = expense.ExpenseType,
        Amount = expense.Amount,
        Description = expense.Description
    };
}
