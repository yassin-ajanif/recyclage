using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Recyclage.Views;

namespace Recyclage.Shared.Services;

public sealed class DialogService : IDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var owner = GetOwner();
        if (owner is null)
            return false;

        return await PromptDialog.ShowConfirm(owner, title, message, "نعم", "إلغاء");
    }

    public async Task ShowErrorAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var owner = GetOwner();
        if (owner is null)
            return;

        await PromptDialog.ShowMessage(owner, title, message, "حسنًا");
    }

    private static Window? GetOwner()
        => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
