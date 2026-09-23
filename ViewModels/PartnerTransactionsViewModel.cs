using System.Collections.ObjectModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public class PartnerTransactionsViewModel : PageViewModelBase
{
    public override string Title => "حساب بين الشركاء";

    public ObservableCollection<PartnerTransactionRow> Rows { get; } =
        new(DemoData.PartnerTransactions);
}
