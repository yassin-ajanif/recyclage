using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class ProductRowViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _productType = "للشراء";

    [ObservableProperty]
    private string _defaultUnit = "كغ";

    [ObservableProperty]
    private decimal _defaultUnitPrice;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);
    public bool CanDelete => Id > 0;

    public void MarkAsSaved(int id)
    {
        Id = id;
        OnPropertyChanged(nameof(CanDelete));
    }
}
