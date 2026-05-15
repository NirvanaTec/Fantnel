namespace Nirvana.DevPlugin.Events.Event;

public interface IEventCreateInterceptor {
    bool OnCreateInterceptor(int port);
}