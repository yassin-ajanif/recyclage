using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Recyclage.ViewModels;

namespace Recyclage.Views;

public partial class ProductsView : UserControl
{
    public ProductsView()
    {
        InitializeComponent();
    }

    private async void OnAutoSave(object? sender, RoutedEventArgs e) =>
        await SaveRowFromSenderAsync(sender);

    private async void OnComboBoxSave(object? sender, SelectionChangedEventArgs e) =>
        await SaveRowFromSenderAsync(sender);

    private async Task SaveRowFromSenderAsync(object? sender)
    {
        if (DataContext is not ProductsViewModel vm)
            return;

        if (FindRow(sender) is not ProductRowViewModel row)
            return;

        await vm.SaveRowAsync(row);
    }

    private static ProductRowViewModel? FindRow(object? sender)
    {
        if (sender is not Control control)
            return null;

        var current = control as Control;
        while (current is not null)
        {
            if (current.DataContext is ProductRowViewModel row)
                return row;

            current = current.GetVisualParent() as Control;
        }

        return null;
    }
}
