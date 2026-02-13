using Codexus.Development.SDK.Manager;

namespace Codexus.Development.SDK.Event;

public class EventCreateInterceptor(int port) : IEventArgs {
    public int Port { get; set; } = port;

    public bool IsCancelled { get; set; }
}