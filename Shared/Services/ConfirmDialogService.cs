using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Recyclage.Views;

namespace Recyclage.Shared.Services;

public static class ConfirmDialogService
{
    public static async Task<bool> ConfirmDeleteAsync(string? itemLabel = null)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return false;

        var owner = desktop.MainWindow;
        if (owner is null)
            return false;

        var message = string.IsNullOrWhiteSpace(itemLabel)
            ? "هل أنت متأكد من الحذف؟ لا يمكن التراجع عن هذه العملية."
            : $"هل أنت متأكد من حذف «{itemLabel}»؟ لا يمكن التراجع عن هذه العملية.";

        var dialog = new ConfirmDialog("تأكيد الحذف", message);
        return await dialog.ShowDialog<bool>(owner);
    }
}
