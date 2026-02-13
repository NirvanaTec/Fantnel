using Codexus.Development.SDK.Connection;

namespace Codexus.Development.SDK.Manager;

public abstract class EventArgsBase(GameConnection connection) : IEventArgs {
    public GameConnection Connection { get; set; } = connection;

    public bool IsCancelled { get; set; }
}