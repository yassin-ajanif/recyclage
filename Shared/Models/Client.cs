namespace Recyclage.Shared.Models;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Ice { get; set; } = string.Empty;
}
