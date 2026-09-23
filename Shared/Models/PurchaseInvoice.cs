namespace Recyclage.Shared.Models;

public class PurchaseInvoice : BaseEntity
{
    public string Date { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal TransportCost { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
}
