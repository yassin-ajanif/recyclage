using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Recyclage.ViewModels;

namespace Recyclage.Views;

public partial class ClientsView : UserControl
{
    public ClientsView()
    {
        InitializeComponent();
    }

    private async void OnAutoSave(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ClientsViewModel vm)
            return;

        if (FindRow(sender) is not ClientRowViewModel row)
            return;

        await vm.SaveRowAsync(row);
    }

    private static ClientRowViewModel? FindRow(object? sender)
    {
        if (sender is not Control control)
            return null;

        var current = control as Control;
        while (current is not null)
        {
            if (current.DataContext is ClientRowViewModel row)
                return row;

            current = current.GetVisualParent() as Control;
        }

        return null;
    }
}
