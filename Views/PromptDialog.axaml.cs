using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Recyclage.Views;

/// <summary>
/// Generic prompt: a single-dismiss message box, or a yes/no confirmation when
/// <see cref="ShowConfirm"/> supplies confirm/cancel labels.
/// </summary>
public partial class PromptDialog : Window
{
    public PromptDialog()
    {
        InitializeComponent();
    }

    public PromptDialog(string title, string message) : this()
    {
        Title = title;
        MessageText.Text = message;
    }

    /// <summary>Yes/no prompt. The confirm button is labelled and revealed.</summary>
    public static Task<bool> ShowConfirm(Window owner, string title, string message, string confirmText, string cancelText)
    {
        var dialog = new PromptDialog(title, message)
        {
            ConfirmButton = { Content = confirmText, IsVisible = true },
            DismissButton = { Content = cancelText }
        };

        return dialog.ShowDialog<bool>(owner);
    }

    /// <summary>Message with a single dismiss button.</summary>
    public static Task ShowMessage(Window owner, string title, string message, string dismissText)
    {
        var dialog = new PromptDialog(title, message)
        {
            DismissButton = { Content = dismissText }
        };

        return dialog.ShowDialog<bool>(owner);
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e) => Close(true);

    private void OnDismissClick(object? sender, RoutedEventArgs e) => Close(false);
}
