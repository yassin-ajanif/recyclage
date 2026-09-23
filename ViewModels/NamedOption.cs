namespace Recyclage.ViewModels;

public sealed class NamedOption : IEquatable<NamedOption>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public override string ToString() => Name;

    public bool Equals(NamedOption? other) => other is not null && Id == other.Id;

    public override bool Equals(object? obj) => obj is NamedOption other && Equals(other);

    public override int GetHashCode() => Id;
}
