using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Manager;

namespace Codexus.Development.SDK.Event;

public class EventConnectionClosed(GameConnection connection) : EventArgsBase(connection);