using Codexus.Development.SDK.Enums;

namespace Codexus.Development.SDK.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentLoader(EnumLoaderType type) : Attribute {
    public EnumLoaderType Type { get; } = type;
}