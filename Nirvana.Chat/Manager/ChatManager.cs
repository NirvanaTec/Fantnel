using Codexus.Development.SDK.Event;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Utils;
using Nirvana.Chat.Message;

namespace Nirvana.Chat.Manager;

public static class ChatManager {
    private static bool _status; // 是否注册

    public static void Register()
    {
        if (_status) {
            return;
        }

        _status = true;
        foreach (var channel in MessageChannels.AllVersions) {
            EventManager.Instance.RegisterHandler<EventLoginSuccess>(channel, OnLoginSuccess);
        }

        EventManager.Instance.RegisterHandler<EventConnectionClosed>(MessageChannels.ChannelConnection, OnConnectionClosed);
    }

    private static void OnLoginSuccess(EventLoginSuccess args)
    {
        // 已存在
        ChatMessage.Start(args.Connection);
    }

    private static void OnConnectionClosed(EventConnectionClosed args)
    {
        _ = ChatMessage.RemoveJoin(args.Connection);
    }
}