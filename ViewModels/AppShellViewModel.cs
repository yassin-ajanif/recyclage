using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Recyclage.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    [ObservableProperty]
    private PageViewModelBase? _currentPage;

    [ObservableProperty]
    private bool _invoicesExpanded = true;

    [ObservableProperty]
    private bool _accountsExpanded = true;

    [ObservableProperty]
    private bool _settingsExpanded = true;

    [ObservableProperty]
    private string? _activePageKey;

    public string InvoicesArrow => InvoicesExpanded ? "▼" : "◀";
    public string AccountsArrow => AccountsExpanded ? "▼" : "◀";
    public string SettingsArrow => SettingsExpanded ? "▼" : "◀";

    public AppShellViewModel(IServiceProvider services)
    {
        _services = services;
        GoPurchaseInvoices();
    }

    partial void OnInvoicesExpandedChanged(bool value) => OnPropertyChanged(nameof(InvoicesArrow));
    partial void OnAccountsExpandedChanged(bool value) => OnPropertyChanged(nameof(AccountsArrow));
    partial void OnSettingsExpandedChanged(bool value) => OnPropertyChanged(nameof(SettingsArrow));

    [RelayCommand]
    private void ToggleInvoices() => InvoicesExpanded = !InvoicesExpanded;

    [RelayCommand]
    private void ToggleAccounts() => AccountsExpanded = !AccountsExpanded;

    [RelayCommand]
    private void ToggleSettings() => SettingsExpanded = !SettingsExpanded;

    [RelayCommand]
    private void GoPurchaseInvoices() => Navigate("purchases", () => new PurchaseInvoicesViewModel());

    [RelayCommand]
    private void GoSales() => Navigate("sales", () => new SalesViewModel());

    [RelayCommand]
    private void GoExpenses() => Navigate("expenses", () => new ExpensesViewModel());

    [RelayCommand]
    private void GoMonthlyClosing() => Navigate("monthly", () => new MonthlyClosingReportViewModel());

    [RelayCommand]
    private void GoSupplierReport() => Navigate("supplier-report", () => new SupplierInvoiceReportViewModel());

    [RelayCommand]
    private void GoCustomerReport() => Navigate("customer-report", () => new CustomerInvoiceReportViewModel());

    [RelayCommand]
    private void GoPartnerTransactions() => Navigate("partners", () => new PartnerTransactionsViewModel());

    [RelayCommand]
    private void GoProducts() => Navigate("products", () => _services.GetRequiredService<ProductsViewModel>());

    [RelayCommand]
    private void GoCompanyCapital() => Navigate("capital", () => new CompanyCapitalViewModel());

    private void Navigate(string key, Func<PageViewModelBase> factory)
    {
        ActivePageKey = key;
        CurrentPage = factory();
    }
}
