using Recyclage.Shared.Models;

namespace Recyclage.Shared.Database;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.AppSettings.Any())
        {
            db.AppSettings.Add(new AppSettingsRow { Id = 1 });
            db.SaveChanges();
        }

        if (!db.Suppliers.Any())
        {
            db.Suppliers.AddRange(
                new Supplier { Name = "محمد الفرنيسور" },
                new Supplier { Name = "شركة المعادن" },
                new Supplier { Name = "يوسف التاجر" });
            db.SaveChanges();
        }

        if (!db.Clients.Any())
        {
            db.Clients.AddRange(
                new Client { Name = "أحمد الزبون" },
                new Client { Name = "فاطمة" },
                new Client { Name = "كريم الصناعة" });
            db.SaveChanges();
        }

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Name = "بلاستيك خام", ProductType = ProductType.ForBuying, DefaultUnitPrice = 4m },
                new Product { Name = "حديد خردة", ProductType = ProductType.ForBuying, DefaultUnitPrice = 3.5m },
                new Product { Name = "ورق", ProductType = ProductType.ForBuying, DefaultUnitPrice = 2m },
                new Product { Name = "بلاستيك معاد تدويره", ProductType = ProductType.ForSale, DefaultUnitPrice = 6m },
                new Product { Name = "حديد معاد تدويره", ProductType = ProductType.ForSale, DefaultUnitPrice = 5.5m },
                new Product { Name = "ورق معاد تدويره", ProductType = ProductType.ForSale, DefaultUnitPrice = 3.5m });
            db.SaveChanges();
        }

        SeedDemoPurchaseInvoices(db);
        SeedDemoSales(db);
        SeedDemoExpenses(db);
        SeedDemoPartnerTransactions(db);
    }

    private static void SeedDemoPartnerTransactions(AppDbContext db)
    {
        if (db.PartnerTransactions.Any())
            return;

        db.PartnerTransactions.AddRange(
            new PartnerTransaction { Date = "2026-11-05", PaidByAyoub = 3000m, ReturnedToMustafa = 1500m, Details = "تسوية جزئية" },
            new PartnerTransaction { Date = "2026-11-18", PaidByAyoub = 0m, ReturnedToMustafa = 2000m, Details = "رجوع لمصطفى" },
            new PartnerTransaction { Date = "2026-11-21", PaidByAyoub = 5000m, ReturnedToMustafa = 0m, Details = "دفع مورد بلاستيك" });
        db.SaveChanges();
    }

    private static void SeedDemoExpenses(AppDbContext db)
    {
        if (db.Expenses.Any())
            return;

        db.Expenses.AddRange(
            new Expense { Date = "2026-11-20", ExpenseType = "إيجار", Amount = 3500m, Description = "إيجار المحل" },
            new Expense { Date = "2026-11-17", ExpenseType = "وقود", Amount = 850m, Description = "نقل البضاعة" },
            new Expense { Date = "2026-11-12", ExpenseType = "صيانة", Amount = 1200m, Description = "إصلاح الميزان" });
        db.SaveChanges();
    }

    private static void SeedDemoPurchaseInvoices(AppDbContext db)
    {
        if (db.PurchaseInvoices.Any())
            return;

        var rows = new (string Supplier, string Product, string Date, decimal Qty, decimal Price, decimal Transport, decimal Paid)[]
        {
            ("محمد الفرنيسور", "بلاستيك خام", "2026-11-21", 500, 4m, 200m, 1500m),
            ("شركة المعادن", "حديد خردة", "2026-11-18", 1200, 3.5m, 350m, 4550m),
            ("يوسف التاجر", "ورق", "2026-11-10", 300, 2m, 100m, 400m)
        };

        var invoices = new List<PurchaseInvoice>();
        foreach (var row in rows)
        {
            var supplierId = FindSupplierId(db, row.Supplier);
            var productId = FindProductId(db, row.Product, ProductType.ForBuying);
            if (supplierId is null || productId is null)
                continue;

            var total = row.Qty * row.Price + row.Transport;
            invoices.Add(new PurchaseInvoice
            {
                Date = row.Date,
                Quantity = row.Qty,
                ProductId = productId.Value,
                SupplierId = supplierId.Value,
                UnitPrice = row.Price,
                TransportCost = row.Transport,
                Total = total,
                Paid = row.Paid,
                Remaining = total - row.Paid
            });
        }

        if (invoices.Count == 0)
            return;

        db.PurchaseInvoices.AddRange(invoices);
        db.SaveChanges();
    }

    private static void SeedDemoSales(AppDbContext db)
    {
        if (db.Sales.Any())
            return;

        var rows = new (string Client, string Product, string Date, decimal Qty, decimal Price, decimal Transport, decimal Paid)[]
        {
            ("أحمد الزبون", "بلاستيك معاد تدويره", "2026-11-22", 400, 6m, 150m, 2000m),
            ("كريم الصناعة", "حديد معاد تدويره", "2026-11-19", 800, 5.5m, 0m, 4400m),
            ("فاطمة", "ورق معاد تدويره", "2026-11-15", 200, 3.5m, 80m, 500m)
        };

        var sales = new List<Sale>();
        foreach (var row in rows)
        {
            var clientId = FindClientId(db, row.Client);
            var productId = FindProductId(db, row.Product, ProductType.ForSale);
            if (clientId is null || productId is null)
                continue;

            var total = row.Qty * row.Price + row.Transport;
            sales.Add(new Sale
            {
                Date = row.Date,
                Quantity = row.Qty,
                ProductId = productId.Value,
                ClientId = clientId.Value,
                UnitPrice = row.Price,
                TransportCost = row.Transport,
                Total = total,
                Paid = row.Paid,
                Remaining = total - row.Paid
            });
        }

        if (sales.Count == 0)
            return;

        db.Sales.AddRange(sales);
        db.SaveChanges();
    }

    private static int? FindSupplierId(AppDbContext db, string name) =>
        db.Suppliers.FirstOrDefault(s => s.Name == name)?.Id;

    private static int? FindClientId(AppDbContext db, string name) =>
        db.Clients.FirstOrDefault(c => c.Name == name)?.Id;

    private static int? FindProductId(AppDbContext db, string name, ProductType type) =>
        db.Products.FirstOrDefault(p => p.Name == name && p.ProductType == type)?.Id;
}
