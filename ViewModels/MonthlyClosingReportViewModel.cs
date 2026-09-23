using System.Collections.ObjectModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public class MonthlyClosingReportViewModel : PageViewModelBase
{
    public override string Title => "حساب آخر شهر";

    public ObservableCollection<MonthlyClosingRow> Rows { get; } = new(DemoData.MonthlyClosing);
}
