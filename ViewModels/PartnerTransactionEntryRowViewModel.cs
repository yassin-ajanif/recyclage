using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class PartnerTransactionEntryRowViewModel : DatedEditableRowViewModelBase
{
    private decimal _snapshotPaidByAyoub;
    private decimal _snapshotReturnedToMustafa;
    private string _snapshotDetails = string.Empty;

    [ObservableProperty]
    private decimal _paidByAyoub;

    [ObservableProperty]
    private decimal _returnedToMustafa;

    [ObservableProperty]
    private string _details = string.Empty;

    [ObservableProperty]
    private decimal _leftToAyoub;

    [ObservableProperty]
    private decimal _leftToMustafa;

    public bool IsEmpty =>
        PaidByAyoub <= 0 && ReturnedToMustafa <= 0 && string.IsNullOrWhiteSpace(Details);

    public void SetBalances(decimal leftToAyoub, decimal leftToMustafa)
    {
        LeftToAyoub = leftToAyoub;
        LeftToMustafa = leftToMustafa;
    }

    protected override void CaptureSnapshot()
    {
        CaptureDateSnapshot();
        _snapshotPaidByAyoub = PaidByAyoub;
        _snapshotReturnedToMustafa = ReturnedToMustafa;
        _snapshotDetails = Details;
    }

    protected override void RestoreSnapshot()
    {
        RestoreDateSnapshot();
        PaidByAyoub = _snapshotPaidByAyoub;
        ReturnedToMustafa = _snapshotReturnedToMustafa;
        Details = _snapshotDetails;
    }

    protected override void ClearFields()
    {
        ClearDateField();
        PaidByAyoub = 0;
        ReturnedToMustafa = 0;
        Details = string.Empty;
        LeftToAyoub = 0;
        LeftToMustafa = 0;
    }
}
