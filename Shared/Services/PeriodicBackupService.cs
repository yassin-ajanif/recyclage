using System.Timers;
using Timer = System.Timers.Timer;

namespace Recyclage.Shared.Services;

public sealed class PeriodicBackupService : IPeriodicBackupService, IDisposable
{
    private readonly IAppSettingsService _settings;
    private readonly IBackupService _backup;
    private Timer? _timer;

    public PeriodicBackupService(IAppSettingsService settings, IBackupService backup)
    {
        _settings = settings;
        _backup = backup;
    }

    public void Start()
    {
        Stop();
        _timer = new Timer(TimeSpan.FromSeconds(30)) { AutoReset = true };
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    public void Stop()
    {
        if (_timer is null)
            return;

        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();
        _timer = null;
    }

    private async void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            var settings = await _settings.GetAsync();
            if (!settings.BackupEnabled)
                return;

            var now = DateTime.UtcNow;
            var threshold = settings.BackupIntervalUnit == "Minutes"
                ? TimeSpan.FromMinutes(settings.BackupIntervalHours)
                : TimeSpan.FromHours(settings.BackupIntervalHours);

            if (settings.LastBackupDate.HasValue &&
                now - settings.LastBackupDate.Value < threshold)
                return;

            var result = await _backup.CreateBackupAsync(settings.BackupDirectory);
            if (result is null)
                return;

            settings.LastBackupDate = now;
            await _settings.SaveAsync(settings);
            await _backup.CleanupOldBackupsAsync(settings.BackupDirectory, settings.BackupRetentionDays);
        }
        catch
        {
            // silent — don't crash the app on backup failure
        }
    }

    public void Dispose() => Stop();
}
