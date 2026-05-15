using Nirvana.DevPlugin.Entities;

namespace Nirvana.DevPlugin.Events.Event;

public interface IEventParseAddress {
    void OnParseAddress(InterceptorConfig config);
}