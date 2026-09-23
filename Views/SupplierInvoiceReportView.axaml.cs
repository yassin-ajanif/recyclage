using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Recyclage.ViewModels;

namespace Recyclage.Views;

public partial class SupplierInvoiceReportView : UserControl
{
    public SupplierInvoiceReportView()
    {
        InitializeComponent();
    }

    private async void OnAutoSave(object? sender, RoutedEventArgs e) =>
        await SaveRowFromSenderAsync(sender);

    private async void OnProductChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        await SaveRowFromSenderAsync(sender, applyProduct: true);
    }

    private async Task SaveRowFromSenderAsync(object? sender, bool applyProduct = false)
    {
        if (DataContext is not SupplierInvoiceReportViewModel vm)
            return;

        if (FindRow(sender) is not PurchaseInvoiceEntryRowViewModel row)
            return;

        if (applyProduct && sender is ComboBox combo)
            vm.ApplyProductSelection(row, combo.SelectedItem as string);

        await vm.SaveRowAsync(row);
    }

    private static PurchaseInvoiceEntryRowViewModel? FindRow(object? sender)
    {
        if (sender is not Control control)
            return null;

        var current = control as Control;
        while (current is not null)
        {
            if (current.DataContext is PurchaseInvoiceEntryRowViewModel row)
                return row;

            current = current.GetVisualParent() as Control;
        }

        return null;
    }
}
