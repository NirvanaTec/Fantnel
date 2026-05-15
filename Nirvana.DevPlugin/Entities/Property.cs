namespace Nirvana.DevPlugin.Entities;

public class Property {
    public required string Name { get; init; }

    public required string Value { get; init; }

    public required string? Signature { get; init; }
}