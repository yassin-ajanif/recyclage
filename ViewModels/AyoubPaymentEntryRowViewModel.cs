using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class AyoubPaymentEntryRowViewModel : DatedEditableRowViewModelBase
{
    private decimal _snapshotAmount;
    private string _snapshotDetails = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _details = string.Empty;

    public bool IsEmpty => Amount <= 0 && string.IsNullOrWhiteSpace(Details);

    protected override void CaptureSnapshot()
    {
        CaptureDateSnapshot();
        _snapshotAmount = Amount;
        _snapshotDetails = Details;
    }

    protected override void RestoreSnapshot()
    {
        RestoreDateSnapshot();
        Amount = _snapshotAmount;
        Details = _snapshotDetails;
    }

    protected override void ClearFields()
    {
        ClearDateField();
        Amount = 0;
        Details = string.Empty;
    }
}
