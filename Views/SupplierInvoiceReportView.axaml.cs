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

    private void OnProductChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        if (DataContext is not SupplierInvoiceReportViewModel vm)
            return;

        if (FindRow(sender) is not PurchaseInvoiceEntryRowViewModel row)
            return;

        if (sender is ComboBox combo)
            vm.ApplyProductSelection(row, combo.SelectedItem as string);
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
