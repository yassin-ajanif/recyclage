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
    private bool _accountsExpanded;

    [ObservableProperty]
    private bool _settingsExpanded;

    [ObservableProperty]
    private string? _activePageKey;

    public AppShellViewModel(IServiceProvider services)
    {
        _services = services;
        GoSupplierReport();
    }

    [RelayCommand]
    private void ToggleInvoices() => ToggleSection("invoices");

    [RelayCommand]
    private void ToggleAccounts() => ToggleSection("accounts");

    [RelayCommand]
    private void ToggleSettings() => ToggleSection("settings");

    [RelayCommand]
    private void GoPurchaseInvoices() => Navigate("purchases", () => _services.GetRequiredService<PurchaseInvoicesViewModel>());

    [RelayCommand]
    private void GoSales() => Navigate("sales", () => _services.GetRequiredService<SalesViewModel>());

    [RelayCommand]
    private void GoExpenses() => Navigate("expenses", () => _services.GetRequiredService<ExpensesViewModel>());

    [RelayCommand]
    private void GoMonthlyClosing() => Navigate("monthly", () => _services.GetRequiredService<MonthlyClosingReportViewModel>());

    [RelayCommand]
    private void GoSupplierReport() => Navigate("supplier-report", () => _services.GetRequiredService<SupplierInvoiceReportViewModel>());

    [RelayCommand]
    private void GoCustomerReport() => Navigate("customer-report", () => _services.GetRequiredService<CustomerInvoiceReportViewModel>());

    [RelayCommand]
    private void GoPartnerTransactions() => Navigate("partners", () => new PartnerTransactionsViewModel());

    [RelayCommand]
    private void GoProducts() => Navigate("products", () => _services.GetRequiredService<ProductsViewModel>());

    [RelayCommand]
    private void GoClients() => Navigate("clients", () => _services.GetRequiredService<ClientsViewModel>());

    [RelayCommand]
    private void GoSuppliers() => Navigate("suppliers", () => _services.GetRequiredService<SuppliersViewModel>());

    [RelayCommand]
    private void GoCompanyCapital() => Navigate("capital", () => _services.GetRequiredService<CompanyCapitalViewModel>());

    private void Navigate(string key, Func<PageViewModelBase> factory)
    {
        ExpandForPageKey(key);
        ActivePageKey = key;
        CurrentPage = factory();
    }

    private void ToggleSection(string section)
    {
        if (IsSectionExpanded(section))
        {
            InvoicesExpanded = false;
            AccountsExpanded = false;
            SettingsExpanded = false;
            return;
        }

        ExpandOnly(section);
    }

    private void ExpandForPageKey(string key)
    {
        var section = key switch
        {
            "supplier-report" or "customer-report" or "expenses" => "invoices",
            "purchases" or "sales" or "monthly" or "partners" => "accounts",
            "products" or "suppliers" or "clients" or "capital" => "settings",
            _ => null
        };

        if (section is not null)
            ExpandOnly(section);
    }

    private void ExpandOnly(string section)
    {
        InvoicesExpanded = section == "invoices";
        AccountsExpanded = section == "accounts";
        SettingsExpanded = section == "settings";
    }

    private bool IsSectionExpanded(string section) => section switch
    {
        "invoices" => InvoicesExpanded,
        "accounts" => AccountsExpanded,
        "settings" => SettingsExpanded,
        _ => false
    };
}
