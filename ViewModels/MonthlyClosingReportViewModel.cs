using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Recyclage.Models;
using Recyclage.Shared.Database;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class MonthlyClosingReportViewModel : MonthFilteredPageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IAppSettingsService _settings;

    public override string Title => "حساب آخر شهر";

    public ObservableCollection<MonthlyClosingRow> Rows { get; } = [];

    [ObservableProperty]
    private string? _statusMessage;

    public MonthlyClosingReportViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        IAppSettingsService settings)
    {
        _dbFactory = dbFactory;
        _settings = settings;
        _ = LoadAsync();
    }

    protected override void OnMonthFilterChanged() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var settings = await _settings.GetAsync();
            var monthPrefix = SelectedMonthPrefix;
            var endOfMonth = EndOfMonthDate(SelectedYear, SelectedMonth);

            var sales = await db.Sales
                .AsNoTracking()
                .Where(s => s.Date.StartsWith(monthPrefix))
                .SumAsync(s => s.Total);

            var expenses = await db.Expenses
                .AsNoTracking()
                .Where(e => e.Date.StartsWith(monthPrefix))
                .SumAsync(e => e.Amount);

            var purchases = await db.PurchaseInvoices
                .AsNoTracking()
                .Where(p => p.Date.StartsWith(monthPrefix))
                .SumAsync(p => p.Total);

            // PartnerTransactions is not persisted yet; outgoing stays 0 until that table exists.
            var outgoing = 0m;

            var totalPurchasesExpenses = purchases + expenses;
            var monthlyNet = sales - expenses - purchases - outgoing;

            var cumulativeIncome = await db.Sales
                .AsNoTracking()
                .Where(s => string.Compare(s.Date, endOfMonth) <= 0)
                .SumAsync(s => s.Total);

            var cumulativeExpenses = await db.Expenses
                .AsNoTracking()
                .Where(e => string.Compare(e.Date, endOfMonth) <= 0)
                .SumAsync(e => e.Amount);

            var cumulativePurchases = await db.PurchaseInvoices
                .AsNoTracking()
                .Where(p => string.Compare(p.Date, endOfMonth) <= 0)
                .SumAsync(p => p.Total);

            var cumulativeNet = cumulativeIncome - cumulativeExpenses - cumulativePurchases - outgoing;
            var capitalRemaining = settings.CompanyCapital + cumulativeNet;

            Rows.Clear();
            Rows.Add(new MonthlyClosingRow
            {
                Month = MonthFilterOption.FormatMonthYear(SelectedYear, SelectedMonth),
                MonthlyPurchases = FormatMoney(purchases),
                Expenses = FormatMoney(expenses),
                TotalIncomePurchases = FormatMoney(totalPurchasesExpenses),
                Outgoing = FormatMoney(outgoing),
                CapitalRemaining = FormatMoney(capitalRemaining),
                CompanyBalance = FormatSignedMoney(monthlyNet)
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل حساب الشهر: {ex.Message}";
            Rows.Clear();
        }
    }

    private static string EndOfMonthDate(int year, int month) =>
        $"{year:D4}-{month:D2}-{DateTime.DaysInMonth(year, month):D2}";

    private static string FormatMoney(decimal amount) => $"{amount:0.00} د.م";

    private static string FormatSignedMoney(decimal amount) =>
        amount >= 0 ? $"+ {amount:0.00} د.م" : $"- {Math.Abs(amount):0.00} د.م";
}
