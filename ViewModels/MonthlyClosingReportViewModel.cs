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
            var appSettings = await _settings.GetAsync();
            var monthPrefix = SelectedMonthPrefix;

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

            var totalPurchasesExpenses = purchases + expenses;
            var monthlyRemaining = sales - totalPurchasesExpenses;

            var endOfMonth = new DateTime(SelectedYear, SelectedMonth, DateTime.DaysInMonth(SelectedYear, SelectedMonth))
                .ToString("yyyy-MM-dd");
            var cumulativeSales = await db.Sales
                .AsNoTracking()
                .Where(s => s.Date.CompareTo(endOfMonth) <= 0)
                .SumAsync(s => s.Total);
            var cumulativePurchases = await db.PurchaseInvoices
                .AsNoTracking()
                .Where(p => p.Date.CompareTo(endOfMonth) <= 0)
                .SumAsync(p => p.Total);
            var cumulativeExpenses = await db.Expenses
                .AsNoTracking()
                .Where(e => e.Date.CompareTo(endOfMonth) <= 0)
                .SumAsync(e => e.Amount);

            var cumulativeRemaining = cumulativeSales - cumulativePurchases - cumulativeExpenses;
            var companyCapital = appSettings.CompanyCapital + cumulativeRemaining;

            Rows.Clear();
            Rows.Add(new MonthlyClosingRow
            {
                Month = MonthFilterOption.FormatMonthYear(SelectedYear, SelectedMonth),
                MonthlyPurchases = FormatMoney(purchases),
                Expenses = FormatMoney(expenses),
                TotalIncomePurchases = FormatMoney(totalPurchasesExpenses),
                MonthlySales = FormatMoney(sales),
                CapitalRemaining = FormatMoney(monthlyRemaining),
                CompanyBalance = FormatSignedMoney(companyCapital)
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل حساب الشهر: {ex.Message}";
            Rows.Clear();
        }
    }

    private static string FormatMoney(decimal amount) => $"{amount:0.00} د.م";

    private static string FormatSignedMoney(decimal amount) =>
        amount >= 0 ? $"+ {amount:0.00} د.م" : $"- {Math.Abs(amount):0.00} د.م";
}
