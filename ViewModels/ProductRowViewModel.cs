using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class ProductRowViewModel : EditableRowViewModelBase
{
    private string _snapshotName = string.Empty;
    private string _snapshotProductType = "للشراء";
    private string _snapshotDefaultUnit = "كغ";
    private decimal _snapshotDefaultUnitPrice;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _productType = "للشراء";

    [ObservableProperty]
    private string _defaultUnit = "كغ";

    [ObservableProperty]
    private decimal _defaultUnitPrice;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);

    protected override void CaptureSnapshot()
    {
        _snapshotName = Name;
        _snapshotProductType = ProductType;
        _snapshotDefaultUnit = DefaultUnit;
        _snapshotDefaultUnitPrice = DefaultUnitPrice;
    }

    protected override void RestoreSnapshot()
    {
        Name = _snapshotName;
        ProductType = _snapshotProductType;
        DefaultUnit = _snapshotDefaultUnit;
        DefaultUnitPrice = _snapshotDefaultUnitPrice;
    }

    protected override void ClearFields()
    {
        Name = string.Empty;
        ProductType = "للشراء";
        DefaultUnit = "كغ";
        DefaultUnitPrice = 0;
    }
}
