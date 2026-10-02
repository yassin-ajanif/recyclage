using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class PurchaseInvoiceEntryRowViewModel : DatedEditableRowViewModelBase
{
    private string _snapshotProductName = string.Empty;
    private decimal _snapshotQuantity;
    private int? _snapshotProductId;
    private decimal _snapshotUnitPrice;
    private decimal _snapshotTransportPaidByMe;
    private decimal _snapshotTransportPaidByPartner;
    private decimal _snapshotPaid;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    /// <summary>Null when the line is a cash amount only (no product).</summary>
    [ObservableProperty]
    private int? _productId;

    [ObservableProperty]
    private decimal _unitPrice;

    /// <summary>Transport we pay ourselves. Added into <see cref="Total"/>.</summary>
    [ObservableProperty]
    private decimal _transportPaidByMe;

    /// <summary>
    /// Transport the supplier pays. Kept out of <see cref="Total"/>. Filling this
    /// clears <see cref="TransportPaidByMe"/>, so the two never carry a value at once.
    /// </summary>
    [ObservableProperty]
    private decimal _transportPaidByPartner;

    [ObservableProperty]
    private decimal _paid;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _remaining;

    public ObservableCollection<string> ProductNames { get; private set; } = [];

    public bool HasProduct => !string.IsNullOrWhiteSpace(ProductName);

    /// <summary>Product name, or «مبلغ نقدي» for a cash-only line.</summary>
    public string ProductLabel => InvoiceLineLabels.ProductOrFallback(ProductName);

    /// <summary>
    /// Empty means "nothing entered": no product and no amount. Any non-zero amount
    /// counts as entered, so negative and zero values stay allowed.
    /// </summary>
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Date)
        || (string.IsNullOrWhiteSpace(ProductName)
            && Quantity == 0
            && UnitPrice == 0
            && TransportPaidByMe == 0
            && TransportPaidByPartner == 0
            && Paid == 0);

    /// <summary>
    /// Only one transport field may carry a value. Typing in either box zeroes the
    /// other, so this should stay false in normal use; the save path rejects it as a
    /// safety net rather than silently dropping an amount.
    /// </summary>
    public bool HasBothTransportFields => TransportPaidByMe != 0m && TransportPaidByPartner != 0m;

    public void AttachProductNames(ObservableCollection<string> productNames) => ProductNames = productNames;

    partial void OnProductNameChanged(string value)
    {
        // Clearing the product turns the line into a cash amount, so the saved id must go too.
        if (string.IsNullOrWhiteSpace(value))
            ProductId = null;

        OnPropertyChanged(nameof(HasProduct));
        OnPropertyChanged(nameof(ProductLabel));
        RecalculateTotals();
    }

    /// <summary>
    /// A product line totals quantity × unit price plus the transport we pay
    /// ourselves. Transport paid by the supplier is deliberately left out — it never
    /// adds to what this invoice costs us. A line with no product records money handed
    /// over, so it has no invoice value of its own: the total stays zero and the handed
    /// amount lands on Remaining as a negative balance (hand 100 → Total 0,
    /// Remaining −100).
    /// </summary>
    public void RecalculateTotals()
    {
        Total = HasProduct ? Quantity * UnitPrice + TransportPaidByMe : 0m;
        Remaining = Total - Paid;
    }

    partial void OnQuantityChanged(decimal value) => RecalculateTotals();
    partial void OnUnitPriceChanged(decimal value) => RecalculateTotals();
    partial void OnPaidChanged(decimal value) => RecalculateTotals();

    partial void OnTransportPaidByMeChanged(decimal value)
    {
        // Filling one transport box empties the other. Clearing a box leaves the other
        // alone, so zeroing a field never wipes the amount already recorded there.
        if (value != 0m)
            TransportPaidByPartner = 0m;
        RecalculateTotals();
    }

    partial void OnTransportPaidByPartnerChanged(decimal value)
    {
        if (value != 0m)
            TransportPaidByMe = 0m;
        RecalculateTotals();
    }

    protected override void CaptureSnapshot()
    {
        CaptureDateSnapshot();
        _snapshotProductName = ProductName;
        _snapshotQuantity = Quantity;
        _snapshotProductId = ProductId;
        _snapshotUnitPrice = UnitPrice;
        _snapshotTransportPaidByMe = TransportPaidByMe;
        _snapshotTransportPaidByPartner = TransportPaidByPartner;
        _snapshotPaid = Paid;
    }

    protected override void RestoreSnapshot()
    {
        RestoreDateSnapshot();
        ProductName = _snapshotProductName;
        Quantity = _snapshotQuantity;
        ProductId = _snapshotProductId;
        UnitPrice = _snapshotUnitPrice;
        TransportPaidByMe = _snapshotTransportPaidByMe;
        TransportPaidByPartner = _snapshotTransportPaidByPartner;
        Paid = _snapshotPaid;
        RecalculateTotals();
    }

    protected override void ClearFields()
    {
        ClearDateField();
        ProductName = string.Empty;
        Quantity = 0;
        ProductId = null;
        UnitPrice = 0;
        TransportPaidByMe = 0;
        TransportPaidByPartner = 0;
        Paid = 0;
        RecalculateTotals();
    }
}
