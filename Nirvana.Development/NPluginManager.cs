using Codexus.Development.SDK.Event;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Utils;
using Nirvana.Development.Handlers;
using Serilog;

namespace Nirvana.Development;

public class NPluginManager {
    private static bool _status; // 是否注册

    public static void Initialization()
    {
        if (_status) {
            return;
        }

        _status = true;

        foreach (var channel in MessageChannels.AllVersions) {
            EventManager.Instance.RegisterHandler<EventLoginSuccess>(channel, OnLoginSuccess);
        }
    }

    private static void OnLoginSuccess(EventLoginSuccess args)
    {
        var serverChannel = args.Connection.ServerChannel;
        if (serverChannel == null) {
            Log.Fatal("Load Nirvana Plugin Failed, ServerChannel is null");
            return;
        }

        if (args.Connection.ClientChannel.Pipeline.Context("Nirvana_Handler") == null) {
            args.Connection.ClientChannel.Pipeline.AddBefore("handler", "Nirvana_Handler", new ServerHandler(args.Connection));
        }

        if (serverChannel.Pipeline.Context("Nirvana_Handler") == null) {
            serverChannel.Pipeline.AddBefore("handler", "Nirvana_Handler", new ClientHandler(args.Connection));
        }
    }
}