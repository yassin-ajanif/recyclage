namespace Recyclage.ViewModels;

public sealed class NamedOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public override string ToString() => Name;
}
