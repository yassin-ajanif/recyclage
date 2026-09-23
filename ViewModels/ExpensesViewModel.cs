using System.Collections.ObjectModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public class ExpensesViewModel : PageViewModelBase
{
    public override string Title => "فاتورة المصاريف";

    public ObservableCollection<ExpenseRow> Rows { get; } = new(DemoData.Expenses);
}
