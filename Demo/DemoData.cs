using Recyclage.Models;

namespace Recyclage.Demo;

public static class DemoData
{
    public static IReadOnlyList<string> Suppliers { get; } =
    [
        "محمد الفرنيسور",
        "شركة المعادن",
        "يوسف التاجر"
    ];

    public static IReadOnlyList<string> Clients { get; } =
    [
        "أحمد الزبون",
        "فاطمة",
        "كريم الصناعة"
    ];

    public static IReadOnlyList<PurchaseInvoiceRow> PurchaseInvoices { get; } =
    [
        new()
        {
            Date = "2026-11-21",
            Quantity = 500,
            Product = "بلاستيك خام",
            Supplier = "محمد الفرنيسور",
            UnitPrice = "4,00 د.م",
            TransportCost = "200,00 د.م",
            Total = "2 200,00 د.م",
            Paid = "1 500,00 د.م",
            Remaining = "700,00 د.م"
        },
        new()
        {
            Date = "2026-11-18",
            Quantity = 1200,
            Product = "حديد خردة",
            Supplier = "شركة المعادن",
            UnitPrice = "3,50 د.م",
            TransportCost = "350,00 د.م",
            Total = "4 550,00 د.م",
            Paid = "4 550,00 د.م",
            Remaining = "0,00 د.م"
        },
        new()
        {
            Date = "2026-11-10",
            Quantity = 300,
            Product = "ورق",
            Supplier = "يوسف التاجر",
            UnitPrice = "2,00 د.م",
            TransportCost = "100,00 د.م",
            Total = "700,00 د.م",
            Paid = "400,00 د.م",
            Remaining = "300,00 د.م"
        }
    ];

    public static IReadOnlyList<SaleRow> Sales { get; } =
    [
        new()
        {
            Date = "2026-11-22",
            Quantity = 400,
            Product = "بلاستيك معاد تدويره",
            Client = "أحمد الزبون",
            UnitPrice = "6,00 د.م",
            TransportCost = "150,00 د.م",
            Total = "2 550,00 د.م",
            Paid = "2 000,00 د.م",
            Remaining = "550,00 د.م"
        },
        new()
        {
            Date = "2026-11-19",
            Quantity = 800,
            Product = "حديد معاد تدويره",
            Client = "كريم الصناعة",
            UnitPrice = "5,50 د.م",
            TransportCost = "0,00 د.م",
            Total = "4 400,00 د.م",
            Paid = "4 400,00 د.م",
            Remaining = "0,00 د.م"
        },
        new()
        {
            Date = "2026-11-15",
            Quantity = 200,
            Product = "ورق معاد تدويره",
            Client = "فاطمة",
            UnitPrice = "3,50 د.م",
            TransportCost = "80,00 د.م",
            Total = "780,00 د.م",
            Paid = "500,00 د.م",
            Remaining = "280,00 د.م"
        }
    ];

    public static IReadOnlyList<ExpenseRow> Expenses { get; } =
    [
        new() { Date = "2026-11-20", ExpenseType = "إيجار", Amount = "3 500,00 د.م", Description = "إيجار المحل" },
        new() { Date = "2026-11-17", ExpenseType = "وقود", Amount = "850,00 د.م", Description = "نقل البضاعة" },
        new() { Date = "2026-11-12", ExpenseType = "صيانة", Amount = "1 200,00 د.م", Description = "إصلاح الميزان" }
    ];

    public static IReadOnlyList<PartnerTransactionRow> PartnerTransactions { get; } =
    [
        new()
        {
            Date = "2026-11-21",
            PaidByAyoub = "5 000,00 د.م",
            ReturnedToMustafa = "0,00 د.م",
            Details = "دفع مورد بلاستيك",
            LeftToAyoub = "12 500,00 د.م",
            LeftToMustafa = "8 000,00 د.م"
        },
        new()
        {
            Date = "2026-11-18",
            PaidByAyoub = "0,00 د.م",
            ReturnedToMustafa = "2 000,00 د.م",
            Details = "رجوع لمصطفى",
            LeftToAyoub = "7 500,00 د.م",
            LeftToMustafa = "10 000,00 د.م"
        },
        new()
        {
            Date = "2026-11-05",
            PaidByAyoub = "3 000,00 د.م",
            ReturnedToMustafa = "1 500,00 د.م",
            Details = "تسوية جزئية",
            LeftToAyoub = "7 500,00 د.م",
            LeftToMustafa = "8 000,00 د.م"
        }
    ];

    public static IReadOnlyList<MonthlyClosingRow> MonthlyClosing { get; } =
    [
        new()
        {
            Month = "نوفمبر 2026",
            MonthlyPurchases = "7 450,00 د.م",
            Expenses = "5 550,00 د.م",
            TotalIncomePurchases = "13 000,00 د.م",
            Outgoing = "3 500,00 د.م"
        }
    ];
}
