using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class PurchaseInvoiceEntryRowViewModel : DatedEditableRowViewModelBase
{
    private string _snapshotProductName = string.Empty;
    private decimal _snapshotQuantity;
    private int? _snapshotProductId;
    private decimal _snapshotUnitPrice;
    private decimal _snapshotTransportCost;
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

    [ObservableProperty]
    private decimal _transportCost;

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
            && TransportCost == 0
            && Paid == 0);

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
    /// A product line totals quantity × unit price plus transport. A line with no
    /// product records money handed over, so it has no invoice value of its own:
    /// the total stays zero and the handed amount lands on Remaining as a negative
    /// balance (hand 100 → Total 0, Remaining −100).
    /// </summary>
    public void RecalculateTotals()
    {
        Total = HasProduct ? Quantity * UnitPrice + TransportCost : 0m;
        Remaining = Total - Paid;
    }

    partial void OnQuantityChanged(decimal value) => RecalculateTotals();
    partial void OnUnitPriceChanged(decimal value) => RecalculateTotals();
    partial void OnTransportCostChanged(decimal value) => RecalculateTotals();
    partial void OnPaidChanged(decimal value) => RecalculateTotals();

    protected override void CaptureSnapshot()
    {
        CaptureDateSnapshot();
        _snapshotProductName = ProductName;
        _snapshotQuantity = Quantity;
        _snapshotProductId = ProductId;
        _snapshotUnitPrice = UnitPrice;
        _snapshotTransportCost = TransportCost;
        _snapshotPaid = Paid;
    }

    protected override void RestoreSnapshot()
    {
        RestoreDateSnapshot();
        ProductName = _snapshotProductName;
        Quantity = _snapshotQuantity;
        ProductId = _snapshotProductId;
        UnitPrice = _snapshotUnitPrice;
        TransportCost = _snapshotTransportCost;
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
        TransportCost = 0;
        Paid = 0;
        RecalculateTotals();
    }
}
