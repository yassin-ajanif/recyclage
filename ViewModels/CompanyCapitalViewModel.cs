using Recyclage.Demo;

namespace Recyclage.ViewModels;

public class CompanyCapitalViewModel : PageViewModelBase
{
    public override string Title => "رأس المال";

    public string CompanyCapital { get; } = DemoData.CompanyCapital;
}
