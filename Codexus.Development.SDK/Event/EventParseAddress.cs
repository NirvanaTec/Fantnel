using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Manager;

namespace Codexus.Development.SDK.Event;

public class EventParseAddress(GameConnection connection, string address, int port) : EventArgsBase(connection) {
    public string Address { get; set; } = address;
    public int Port { get; set; } = port;
}