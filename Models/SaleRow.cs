namespace Recyclage.Models;

public class SaleRow
{
    public string Date { get; init; } = "";
    public decimal Quantity { get; init; }
    public string Product { get; init; } = "";
    public string Client { get; init; } = "";
    public string UnitPrice { get; init; } = "";
    public string TransportPaidByMe { get; init; } = "";
    public string TransportPaidByPartner { get; init; } = "";
    public string Total { get; init; } = "";
    public string Paid { get; init; } = "";
    public string Remaining { get; init; } = "";
}
