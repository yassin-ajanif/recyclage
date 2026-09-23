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
    }
}
