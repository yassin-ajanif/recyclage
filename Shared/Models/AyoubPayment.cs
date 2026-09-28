namespace Recyclage.Shared.Models;

public class AyoubPayment : BaseEntity
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Details { get; set; } = string.Empty;
}
