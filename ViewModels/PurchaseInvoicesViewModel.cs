using System.Collections.ObjectModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public class PurchaseInvoicesViewModel : PageViewModelBase
{
    public override string Title => "فاتورة المشتريات";

    public ObservableCollection<PurchaseInvoiceRow> Rows { get; } =
        new(DemoData.PurchaseInvoices);
}
