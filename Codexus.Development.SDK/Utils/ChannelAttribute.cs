using Codexus.Development.SDK.Connection;
using DotNetty.Common.Utilities;

namespace Codexus.Development.SDK.Utils;

public static class ChannelAttribute {
    public static readonly AttributeKey<GameConnection> Connection = AttributeKey<GameConnection>.ValueOf("Connection");
}