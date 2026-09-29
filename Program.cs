using Avalonia;
using Velopack;

namespace Recyclage;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Must run before Avalonia initialises. On an update launch Velopack
        // handles the installer arguments; on a normal launch it is a no-op.
        VelopackApp.Build().Run();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
