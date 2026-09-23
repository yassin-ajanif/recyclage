using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Recyclage.ViewModels;

namespace Recyclage.Views;

public partial class SuppliersView : UserControl
{
    public SuppliersView()
    {
        InitializeComponent();
    }

    private async void OnAutoSave(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SuppliersViewModel vm)
            return;

        if (FindRow(sender) is not SupplierRowViewModel row)
            return;

        await vm.SaveRowAsync(row);
    }

    private static SupplierRowViewModel? FindRow(object? sender)
    {
        if (sender is not Control control)
            return null;

        var current = control as Control;
        while (current is not null)
        {
            if (current.DataContext is SupplierRowViewModel row)
                return row;

            current = current.GetVisualParent() as Control;
        }

        return null;
    }
}
