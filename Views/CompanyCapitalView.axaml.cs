using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Recyclage.ViewModels;

namespace Recyclage.Views;

public partial class CompanyCapitalView : UserControl
{
    public CompanyCapitalView()
    {
        InitializeComponent();
        PickBackupDirButton.Click += OnPickBackupDirectory;
    }

    private async void OnPickBackupDirectory(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CompanyCapitalViewModel vm)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is not { } storage)
            return;

        var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "اختر مجلد النسخ الاحتياطي",
            AllowMultiple = false
        });

        if (folders.Count == 0)
            return;

        var path = folders[0].TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path))
            await vm.SetBackupDirectoryAsync(path);
    }
}
