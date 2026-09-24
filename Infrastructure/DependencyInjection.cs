using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recyclage.Shared.Database;
using Recyclage.Shared.Services;
using Recyclage.ViewModels;

namespace Recyclage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRecyclage(this IServiceCollection services)
    {
        var connectionString = DatabasePath.GetConnectionString();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IPeriodicBackupService, PeriodicBackupService>();

        services.AddSingleton<AppShellViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<SuppliersViewModel>();
        services.AddTransient<CompanyCapitalViewModel>();
        services.AddTransient<SupplierInvoiceReportViewModel>();
        services.AddTransient<CustomerInvoiceReportViewModel>();
        services.AddTransient<PurchaseInvoicesViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<ExpensesViewModel>();
        services.AddTransient<MonthlyClosingReportViewModel>();
        services.AddTransient<PartnerTransactionsViewModel>();

        return services;
    }
}
