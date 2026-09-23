using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class PurchaseInvoiceEntryRowViewModel : DatedEditableRowViewModelBase
{
    private string _snapshotProductName = string.Empty;
    private decimal _snapshotQuantity;
    private int _snapshotProductId;
    private decimal _snapshotUnitPrice;
    private decimal _snapshotTransportCost;
    private decimal _snapshotPaid;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private int _productId;

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

    public bool IsEmpty => string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(Date);

    public void AttachProductNames(ObservableCollection<string> productNames) => ProductNames = productNames;

    public void RecalculateTotals()
    {
        Total = Quantity * UnitPrice + TransportCost;
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
        ProductId = 0;
        UnitPrice = 0;
        TransportCost = 0;
        Paid = 0;
        RecalculateTotals();
    }
}
