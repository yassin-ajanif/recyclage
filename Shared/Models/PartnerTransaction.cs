namespace Recyclage.Shared.Models;

public class PartnerTransaction : BaseEntity
{
    public string Date { get; set; } = string.Empty;
    public decimal PaidByAyoub { get; set; }
    public decimal ReturnedToMustafa { get; set; }
    public string Details { get; set; } = string.Empty;
}
