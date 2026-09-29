using System.Reflection;
using Recyclage.Shared.Configuration;
using Velopack;
using Velopack.Exceptions;
using Velopack.Sources;

namespace Recyclage.Shared.Services;

/// <summary>
/// Checks GitHub Releases for a newer build, downloads it and applies it on restart.
/// Failures are swallowed: an unreachable update server must never block the app.
/// </summary>
public sealed class AppUpdateService : IAppUpdateService
{
    private readonly IDialogService _dialogService;
    private UpdateManager? _updateManager;
    private UpdateInfo? _pendingUpdateInfo;

    public AppUpdateService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public string DisplayVersion { get; } = ResolveVersion();

    public bool IsInstalled => TryGetManager()?.IsInstalled ?? false;

    public bool IsUpdateAvailable { get; private set; }

    public string? AvailableVersion { get; private set; }

    public bool IsUpdateDownloaded { get; private set; }

    public bool IsCheckingForUpdates { get; private set; }

    public event EventHandler? UpdateStateChanged;

    public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (IsCheckingForUpdates)
            return;

        try
        {
            IsCheckingForUpdates = true;
            NotifyStateChanged();

            var mgr = TryGetManager();
            if (mgr is null || !mgr.IsInstalled)
            {
                ClearUpdateState();
                return;
            }

            // An update finished downloading last time; it is waiting on a restart.
            var pending = mgr.UpdatePendingRestart;
            if (pending is not null)
            {
                SetUpdateAvailable(pending.Version.ToString(), downloaded: true);
                return;
            }

            var updateInfo = await mgr.CheckForUpdatesAsync();
            if (updateInfo is null)
            {
                ClearUpdateState();
                return;
            }

            _pendingUpdateInfo = updateInfo;
            SetUpdateAvailable(updateInfo.TargetFullRelease.Version.ToString(), downloaded: false);
        }
        catch (Exception)
        {
            // Never surface update-check failures in the UI.
        }
        finally
        {
            IsCheckingForUpdates = false;
            NotifyStateChanged();
        }
    }

    public async Task DownloadAndApplyUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var mgr = TryGetManager();
            if (mgr is null || !mgr.IsInstalled)
                return;

            VelopackAsset? asset = mgr.UpdatePendingRestart;
            if (asset is null && _pendingUpdateInfo is not null)
            {
                await mgr.DownloadUpdatesAsync(_pendingUpdateInfo, progress: null, cancellationToken);
                asset = _pendingUpdateInfo;
                IsUpdateDownloaded = true;
                NotifyStateChanged();
            }

            if (asset is null)
                return;

            var restart = await _dialogService.ConfirmAsync(
                "تتوفر نسخة جديدة",
                $"النسخة {asset.Version} جاهزة للتثبيت. هل تريد إعادة التشغيل الآن لتطبيقها؟",
                cancellationToken);

            if (restart)
                mgr.ApplyUpdatesAndRestart(asset);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "تعذّر التحديث",
                $"تعذّر تثبيت النسخة الجديدة: {ex.Message}",
                cancellationToken);
        }
    }

    private void SetUpdateAvailable(string version, bool downloaded)
    {
        IsUpdateAvailable = true;
        AvailableVersion = version;
        IsUpdateDownloaded = downloaded;
        NotifyStateChanged();
    }

    private void ClearUpdateState()
    {
        IsUpdateAvailable = false;
        AvailableVersion = null;
        IsUpdateDownloaded = false;
        _pendingUpdateInfo = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => UpdateStateChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Reads the version from the assembly. Not from Velopack, so a dev build run
    /// straight from bin still reports its real version.
    /// </summary>
    private static string ResolveVersion()
    {
        var assembly = typeof(AppUpdateService).Assembly;

        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            // The SDK can append "+<commit>" metadata; we want just the number.
            var plus = informational.IndexOf('+');
            return plus < 0 ? informational : informational[..plus];
        }

        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    /// <summary>
    /// Building the manager can throw when the app is not a Velopack install
    /// (e.g. running from the IDE), so it is created defensively and cached.
    /// </summary>
    private UpdateManager? TryGetManager()
    {
        if (_updateManager is not null)
            return _updateManager;

        try
        {
            return _updateManager = new UpdateManager(
                new GithubSource(VelopackConfiguration.GitHubRepoUrl, accessToken: null, prerelease: false));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
