using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class SupplierRowViewModel : EditableRowViewModelBase
{
    private string _snapshotName = string.Empty;
    private string _snapshotPhone = string.Empty;
    private string _snapshotIce = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _ice = string.Empty;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);

    protected override void CaptureSnapshot()
    {
        _snapshotName = Name;
        _snapshotPhone = Phone;
        _snapshotIce = Ice;
    }

    protected override void RestoreSnapshot()
    {
        Name = _snapshotName;
        Phone = _snapshotPhone;
        Ice = _snapshotIce;
    }

    protected override void ClearFields()
    {
        Name = string.Empty;
        Phone = string.Empty;
        Ice = string.Empty;
    }
}
