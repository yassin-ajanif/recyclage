namespace Recyclage.Shared.Models;

public class Sale : BaseEntity
{
    public string Date { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    /// <summary>Null for a cash-only line: an amount received from the client with no product.</summary>
    public int? ProductId { get; set; }

    public Product? Product { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public decimal UnitPrice { get; set; }

    /// <summary>Transport we pay ourselves. Added into <see cref="Total"/>.</summary>
    public decimal TransportPaidByMe { get; set; }

    /// <summary>
    /// Transport the client pays. Kept out of <see cref="Total"/> — it is not an
    /// amount added to this invoice. Only one of the two transport fields is filled.
    /// </summary>
    public decimal TransportPaidByPartner { get; set; }

    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
}
