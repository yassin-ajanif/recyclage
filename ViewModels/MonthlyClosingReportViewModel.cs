using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Models;
using Recyclage.Shared.Database;

namespace Recyclage.ViewModels;

public partial class MonthlyClosingReportViewModel : MonthFilteredPageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "حساب آخر شهر";

    public ObservableCollection<MonthlyClosingRow> Rows { get; } = [];

    [ObservableProperty]
    private string? _statusMessage;

    public MonthlyClosingReportViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = LoadAsync();
    }

    protected override void OnMonthFilterChanged() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var income = await db.Sales
                .AsNoTracking()
                .Where(s => s.Date.StartsWith(SelectedMonthPrefix))
                .SumAsync(s => s.Total);

            var expenses = await db.Expenses
                .AsNoTracking()
                .Where(e => e.Date.StartsWith(SelectedMonthPrefix))
                .SumAsync(e => e.Amount);

            var purchases = await db.PurchaseInvoices
                .AsNoTracking()
                .Where(p => p.Date.StartsWith(SelectedMonthPrefix))
                .SumAsync(p => p.Total);

            // PartnerTransactions is not persisted yet; outgoing stays 0 until that table exists.
            var outgoing = 0m;

            Rows.Clear();
            Rows.Add(new MonthlyClosingRow
            {
                Month = MonthFilterOption.FormatMonthYear(SelectedYear, SelectedMonth),
                Income = FormatMoney(income),
                Expenses = FormatMoney(expenses),
                Purchases = FormatMoney(purchases),
                Outgoing = FormatMoney(outgoing)
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل حساب الشهر: {ex.Message}";
            Rows.Clear();
        }
    }

    private static string FormatMoney(decimal amount) => $"{amount:0.00} د.م";
}
