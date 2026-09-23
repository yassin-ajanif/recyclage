namespace Recyclage.Shared.Database;

public static class DatabasePath
{
    public static string GetDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Recyclage");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetConnectionString()
    {
        var dbPath = Path.Combine(GetDirectory(), "data.db");
        return $"Data Source={dbPath}";
    }
}
