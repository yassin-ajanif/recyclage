using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public partial class CustomerInvoiceReportViewModel : PageViewModelBase
{
    public override string Title => "فاتورة زبون";

    public ObservableCollection<string> Clients { get; } = new(DemoData.Clients);

    [ObservableProperty]
    private string? _selectedClient;

    public ObservableCollection<SaleRow> Rows { get; } = [];

    public CustomerInvoiceReportViewModel()
    {
        SelectedClient = Clients.FirstOrDefault();
        RefreshRows();
    }

    partial void OnSelectedClientChanged(string? value) => RefreshRows();

    private void RefreshRows()
    {
        Rows.Clear();
        if (string.IsNullOrWhiteSpace(SelectedClient))
            return;

        foreach (var row in DemoData.Sales.Where(r => r.Client == SelectedClient))
            Rows.Add(row);
    }
}
