using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recyclage.Infrastructure;
using Recyclage.Shared.Database;
using Recyclage.Shared.Services;
using Recyclage.ViewModels;

namespace Recyclage;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            services.AddRecyclage();
            Services = services.BuildServiceProvider();

            using (var db = Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
            {
                db.Database.Migrate();
                DbSeeder.Seed(db);
            }

            Services.GetRequiredService<IPeriodicBackupService>().Start();

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<AppShellViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
