namespace Nirvana.DevPlugin.Events.Event;

public interface IEventCHandshake {
    public bool OnCHandshake(int packetVersion, string serverAddress, ushort serverPort, int nextState);
}