namespace Recyclage.Shared.Models;

public class PurchaseInvoice : BaseEntity
{
    public string Date { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    /// <summary>Null for a cash-only line: an amount handed to the supplier with no product.</summary>
    public int? ProductId { get; set; }

    public Product? Product { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public decimal UnitPrice { get; set; }

    /// <summary>Transport we pay ourselves. Added into <see cref="Total"/>.</summary>
    public decimal TransportPaidByMe { get; set; }

    /// <summary>
    /// Transport the supplier pays. Kept out of <see cref="Total"/> — it is not a
    /// cost added to this invoice. Only one of the two transport fields is filled.
    /// </summary>
    public decimal TransportPaidByPartner { get; set; }

    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
}
