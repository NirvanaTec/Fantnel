using Nirvana.Development.Manager;
using Nirvana.Heypixel.Configuration;
using Serilog;

namespace Nirvana.Heypixel;

public class HeypixelProtocol {
    
    public const string GameId = "4661334467366178884";

    public static void Init()
    {
        Log.Information("[Heypixel] Initializing.");
    }

    static HeypixelProtocol()
    {
        PacketManager.BasePackets.Add(new C2SConfigPluginMessage(), C2SConfigPluginMessage.RegisterPacket);
    }
}