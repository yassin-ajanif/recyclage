namespace Recyclage.Shared.Models;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public string DefaultUnit { get; set; } = "كغ";
    public decimal DefaultUnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
