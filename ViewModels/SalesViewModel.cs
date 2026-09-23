using System.Collections.ObjectModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public class SalesViewModel : PageViewModelBase
{
    public override string Title => "فاتورة المبيعات";

    public ObservableCollection<SaleRow> Rows { get; } = new(DemoData.Sales);
}
