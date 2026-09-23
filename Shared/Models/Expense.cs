namespace Recyclage.Shared.Models;

public class Expense : BaseEntity
{
    public string Date { get; set; } = string.Empty;
    public string ExpenseType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
