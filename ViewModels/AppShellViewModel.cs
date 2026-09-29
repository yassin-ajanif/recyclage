using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IAppUpdateService _updates;

    /// <summary>Window title, carrying the version so it never drifts from the build.</summary>
    public string AppTitle => $"Recyclage — إعادة التدوير v{_updates.DisplayVersion}";

    /// <summary>True once a newer build exists on the release feed.</summary>
    public bool IsUpdateBannerVisible => _updates.IsUpdateAvailable;

    public string UpdateBannerText { get; private set; } = string.Empty;

    [RelayCommand]
    private async Task ApplyUpdateAsync() => await _updates.DownloadAndApplyUpdateAsync();

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

    public AppShellViewModel(IServiceProvider services, IAppUpdateService updates)
    {
        _services = services;
        _updates = updates;
        _updates.UpdateStateChanged += (_, _) => RefreshUpdateBanner();
        RefreshUpdateBanner();
        GoSupplierReport();

        _ = _updates.CheckForUpdatesAsync();
    }

    private void RefreshUpdateBanner()
    {
        if (!_updates.IsUpdateAvailable)
        {
            UpdateBannerText = string.Empty;
            OnPropertyChanged(nameof(IsUpdateBannerVisible));
            OnPropertyChanged(nameof(UpdateBannerText));
            return;
        }

        var version = _updates.AvailableVersion ?? string.Empty;
        UpdateBannerText = _updates.IsUpdateDownloaded
            ? $"النسخة {version} جاهزة — اضغط للتثبيت وإعادة التشغيل"
            : $"تتوفر نسخة جديدة ({version}) — اضغط للتثبيت";

        OnPropertyChanged(nameof(IsUpdateBannerVisible));
        OnPropertyChanged(nameof(UpdateBannerText));
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
    private void GoAyoubPayments() => Navigate("ayoub-payments", () => _services.GetRequiredService<AyoubPaymentsViewModel>());

    [RelayCommand]
    private void GoMustafaReturns() => Navigate("mustafa-returns", () => _services.GetRequiredService<MustafaReturnsViewModel>());

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
            "purchases" or "sales" or "monthly" or "ayoub-payments" or "mustafa-returns" => "accounts",
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
