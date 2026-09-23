namespace Recyclage.Shared.Database;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.AppSettings.Any())
            return;

        db.AppSettings.Add(new AppSettingsRow { Id = 1 });
        db.SaveChanges();
    }
}
