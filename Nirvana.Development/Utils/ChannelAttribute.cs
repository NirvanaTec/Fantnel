using DotNetty.Common.Utilities;
using Nirvana.Development.Connection;

namespace Nirvana.Development.Utils;

public static class ChannelAttribute {
    public static readonly AttributeKey<GameConnection> Connection = AttributeKey<GameConnection>.ValueOf("Connection");
}