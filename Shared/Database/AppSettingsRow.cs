namespace Recyclage.Shared.Database;

/// <summary>Singleton row (Id=1) for application settings.</summary>
public class AppSettingsRow
{
    public int Id { get; set; } = 1;

    public decimal CompanyCapital { get; set; }

    public bool BackupEnabled { get; set; }

    public int BackupIntervalHours { get; set; } = 24;

    /// <summary>"Minutes" or "Hours".</summary>
    public string BackupIntervalUnit { get; set; } = "Hours";

    public int BackupRetentionDays { get; set; } = 30;

    public string BackupDirectory { get; set; } = string.Empty;

    public DateTime? LastBackupDate { get; set; }
}
