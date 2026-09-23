using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class PurchaseInvoiceEntryRowViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _date = DateTime.Today.ToString("yyyy-MM-dd");

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

    [ObservableProperty]
    private NamedOption? _selectedProduct;

    public bool IsEmpty => ProductId == 0 || string.IsNullOrWhiteSpace(Date);
    public bool CanDelete => Id > 0;

    public void MarkAsSaved(int id)
    {
        Id = id;
        OnPropertyChanged(nameof(CanDelete));
    }

    public void RecalculateTotals()
    {
        Total = Quantity * UnitPrice + TransportCost;
        Remaining = Total - Paid;
    }

    partial void OnQuantityChanged(decimal value) => RecalculateTotals();
    partial void OnUnitPriceChanged(decimal value) => RecalculateTotals();
    partial void OnTransportCostChanged(decimal value) => RecalculateTotals();
    partial void OnPaidChanged(decimal value) => RecalculateTotals();
}
