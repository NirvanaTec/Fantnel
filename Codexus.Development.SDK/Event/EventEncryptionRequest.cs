using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Manager;

namespace Codexus.Development.SDK.Event;

public class EventEncryptionRequest(GameConnection connection, string serverId) : EventArgsBase(connection) {
    public string ServerId { get; } = serverId;
}