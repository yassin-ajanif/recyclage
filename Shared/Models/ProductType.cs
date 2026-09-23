namespace Recyclage.Shared.Models;

public enum ProductType
{
    ForBuying,
    ForSale
}

public static class ProductTypeDisplay
{
    public static string ToArabic(ProductType type) => type switch
    {
        ProductType.ForBuying => "للشراء",
        ProductType.ForSale => "للبيع",
        _ => type.ToString()
    };
}
