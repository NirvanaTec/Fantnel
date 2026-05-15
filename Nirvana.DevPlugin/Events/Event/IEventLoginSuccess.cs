namespace Nirvana.DevPlugin.Events.Event;

public interface IEventLoginSuccess {
    bool OnLoginSuccess(BGameConnection connection);
}