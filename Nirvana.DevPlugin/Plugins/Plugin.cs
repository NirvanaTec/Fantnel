using System;

namespace Nirvana.DevPlugin.Plugins;

[AttributeUsage(AttributeTargets.Class)]
public class Plugin(string id, string name, string author, string version, string[]? dependencies = null) : Attribute {
    public string Id { get; } = id;

    public string Name { get; } = name;

    public string Author { get; } = author;

    public string Version { get; } = version;

    public string[]? Dependencies { get; } = dependencies;
}