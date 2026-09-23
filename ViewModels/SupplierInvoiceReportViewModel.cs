using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public partial class SupplierInvoiceReportViewModel : PageViewModelBase
{
    public override string Title => "فاتورة مورد";

    public ObservableCollection<string> Suppliers { get; } = new(DemoData.Suppliers);

    [ObservableProperty]
    private string? _selectedSupplier;

    public ObservableCollection<PurchaseInvoiceRow> Rows { get; } = [];

    public SupplierInvoiceReportViewModel()
    {
        SelectedSupplier = Suppliers.FirstOrDefault();
        RefreshRows();
    }

    partial void OnSelectedSupplierChanged(string? value) => RefreshRows();

    private void RefreshRows()
    {
        Rows.Clear();
        if (string.IsNullOrWhiteSpace(SelectedSupplier))
            return;

        foreach (var row in DemoData.PurchaseInvoices.Where(r => r.Supplier == SelectedSupplier))
            Rows.Add(row);
    }
}
