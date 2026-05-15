using System;
using Nirvana.DevPlugin.Enums;

namespace Nirvana.DevPlugin.Events;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterEvent(EnumProtocolVersion version = EnumProtocolVersion.All) : Attribute {
    public EnumProtocolVersion Version { get; } = version;
}