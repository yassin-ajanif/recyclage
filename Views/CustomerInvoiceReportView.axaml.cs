using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Recyclage.ViewModels;

namespace Recyclage.Views;

public partial class CustomerInvoiceReportView : UserControl
{
    public CustomerInvoiceReportView()
    {
        InitializeComponent();
    }

    private async void OnAutoSave(object? sender, RoutedEventArgs e) =>
        await SaveRowFromSenderAsync(sender);

    private async void OnProductChanged(object? sender, SelectionChangedEventArgs e) =>
        await SaveRowFromSenderAsync(sender, applyProduct: true);

    private async Task SaveRowFromSenderAsync(object? sender, bool applyProduct = false)
    {
        if (DataContext is not CustomerInvoiceReportViewModel vm)
            return;

        if (FindRow(sender) is not SaleEntryRowViewModel row)
            return;

        if (applyProduct && sender is ComboBox combo)
            vm.ApplyProductSelection(row, combo.SelectedItem as NamedOption);

        await vm.SaveRowAsync(row);
    }

    private static SaleEntryRowViewModel? FindRow(object? sender)
    {
        if (sender is not Control control)
            return null;

        var current = control as Control;
        while (current is not null)
        {
            if (current.DataContext is SaleEntryRowViewModel row)
                return row;

            current = current.GetVisualParent() as Control;
        }

        return null;
    }
}
