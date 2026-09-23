using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recyclage.Shared.Database;
using Recyclage.ViewModels;

namespace Recyclage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRecyclage(this IServiceCollection services)
    {
        var connectionString = DatabasePath.GetConnectionString();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddSingleton<AppShellViewModel>();
        services.AddTransient<ProductsViewModel>();

        return services;
    }
}
