namespace Recyclage.Models;

public class PurchaseInvoiceRow
{
    public string Date { get; init; } = "";
    public decimal Quantity { get; init; }
    public string Product { get; init; } = "";
    public string Supplier { get; init; } = "";
    public string UnitPrice { get; init; } = "";
    public string TransportCost { get; init; } = "";
    public string Total { get; init; } = "";
    public string Paid { get; init; } = "";
    public string Remaining { get; init; } = "";
}
