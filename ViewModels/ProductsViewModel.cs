using System.Collections.ObjectModel;
using Recyclage.Demo;
using Recyclage.Models;

namespace Recyclage.ViewModels;

public class ProductsViewModel : PageViewModelBase
{
    public override string Title => "المنتجات";

    public ObservableCollection<ProductRow> Rows { get; } = new(DemoData.Products);
}
