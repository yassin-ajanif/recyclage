using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recyclage.Shared.Database;
using Recyclage.Shared.Services;

namespace Recyclage.ViewModels;

public partial class CompanyCapitalViewModel : PageViewModelBase
{
    private readonly IAppSettingsService _settings;
    private readonly IBackupService _backup;

    public override string Title => "الإعدادات";

    public List<string> BackupIntervalUnitOptions { get; } = ["Minutes", "Hours"];

    [ObservableProperty]
    private decimal _companyCapital;

    [ObservableProperty]
    private bool _backupEnabled;

    [ObservableProperty]
    private int _backupIntervalHours = 24;

    [ObservableProperty]
    private string _backupIntervalUnit = "Hours";

    [ObservableProperty]
    private int _backupRetentionDays = 30;

    [ObservableProperty]
    private string _backupDirectory = string.Empty;

    [ObservableProperty]
    private string _lastBackupDateStr = string.Empty;

    [ObservableProperty]
    private int _backupCount;

    [ObservableProperty]
    private bool _backupExpanded;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public string BackupArrow => BackupExpanded ? "▼" : "◀";

    public CompanyCapitalViewModel(IAppSettingsService settings, IBackupService backup)
    {
        _settings = settings;
        _backup = backup;
        _ = LoadAsync();
    }

    partial void OnBackupExpandedChanged(bool value) => OnPropertyChanged(nameof(BackupArrow));

    [RelayCommand]
    private void ToggleBackup() => BackupExpanded = !BackupExpanded;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var row = await _settings.GetAsync();
            ApplyRow(row);
            BackupCount = await _backup.GetBackupCountAsync(row.BackupDirectory);
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل الإعدادات: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (BackupIntervalHours <= 0)
        {
            StatusMessage = "فترة النسخ الاحتياطي يجب أن تكون أكبر من صفر.";
            return;
        }

        if (BackupRetentionDays < 0)
        {
            StatusMessage = "مدة الاحتفاظ لا يمكن أن تكون سالبة.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var existing = await _settings.GetAsync();
            var row = ToRow(existing.LastBackupDate);
            await _settings.SaveAsync(row);
            StatusMessage = "تم حفظ الإعدادات.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ الإعدادات: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SetBackupDirectoryAsync(string path)
    {
        BackupDirectory = path;
        BackupCount = await _backup.GetBackupCountAsync(path);
        await SaveAsync();
    }

    [RelayCommand]
    private async Task CreateBackupNowAsync()
    {
        if (string.IsNullOrWhiteSpace(BackupDirectory))
        {
            StatusMessage = "اختر مجلد النسخ الاحتياطي أولاً.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _backup.CreateBackupAsync(BackupDirectory);
            if (result is null)
            {
                StatusMessage = "فشل إنشاء النسخة الاحتياطية.";
                return;
            }

            var existing = await _settings.GetAsync();
            existing.LastBackupDate = DateTime.UtcNow;
            existing.CompanyCapital = CompanyCapital;
            existing.BackupEnabled = BackupEnabled;
            existing.BackupIntervalHours = BackupIntervalHours;
            existing.BackupIntervalUnit = BackupIntervalUnit;
            existing.BackupRetentionDays = BackupRetentionDays;
            existing.BackupDirectory = BackupDirectory;
            await _settings.SaveAsync(existing);

            LastBackupDateStr = DateTime.Now.ToString("g");
            BackupCount = await _backup.GetBackupCountAsync(BackupDirectory);
            await _backup.CleanupOldBackupsAsync(BackupDirectory, BackupRetentionDays);
            StatusMessage = "تم إنشاء النسخة الاحتياطية.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"فشل النسخ الاحتياطي: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyRow(AppSettingsRow row)
    {
        CompanyCapital = row.CompanyCapital;
        BackupEnabled = row.BackupEnabled;
        BackupIntervalHours = row.BackupIntervalHours;
        BackupIntervalUnit = string.IsNullOrWhiteSpace(row.BackupIntervalUnit) ? "Hours" : row.BackupIntervalUnit;
        BackupRetentionDays = row.BackupRetentionDays;
        BackupDirectory = row.BackupDirectory;
        LastBackupDateStr = row.LastBackupDate.HasValue
            ? row.LastBackupDate.Value.ToLocalTime().ToString("g")
            : string.Empty;
    }

    private AppSettingsRow ToRow(DateTime? lastBackupDate) => new()
    {
        Id = 1,
        CompanyCapital = CompanyCapital,
        BackupEnabled = BackupEnabled,
        BackupIntervalHours = BackupIntervalHours,
        BackupIntervalUnit = BackupIntervalUnit,
        BackupRetentionDays = BackupRetentionDays,
        BackupDirectory = BackupDirectory,
        LastBackupDate = lastBackupDate
    };
}
