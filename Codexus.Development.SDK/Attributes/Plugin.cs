namespace Codexus.Development.SDK.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class Plugin(
    string id,
    string name,
    string description,
    string author,
    string version,
    string[]? dependencies = null)
    : Attribute {
    public string Id { get; } = id;

    public string Name { get; } = name;

    public string Author { get; } = author;

    public string Description { get; } = description;

    public string Version { get; } = version;

    public string[]? Dependencies { get; } = dependencies;
}