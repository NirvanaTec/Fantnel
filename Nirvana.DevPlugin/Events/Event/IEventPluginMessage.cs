namespace Nirvana.DevPlugin.Events.Event;

public interface IEventPluginMessage {
    string? Identifier { get; set; }
    byte[]? Payload { get; set; }

    /**
     * true: 取消
     * false: 不取消
     */
    bool OnPluginMessage(BGameConnection connection)
    {
        return true;
    }
}