namespace Recyclage.Shared.Services;

public interface IAppUpdateService
{
    /// <summary>Version of the running build, e.g. "1.0.0".</summary>
    string DisplayVersion { get; }

    /// <summary>True when running from a Velopack install rather than a dev build.</summary>
    bool IsInstalled { get; }

    bool IsUpdateAvailable { get; }

    string? AvailableVersion { get; }

    bool IsUpdateDownloaded { get; }

    bool IsCheckingForUpdates { get; }

    event EventHandler? UpdateStateChanged;

    Task CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    Task DownloadAndApplyUpdateAsync(CancellationToken cancellationToken = default);
}
