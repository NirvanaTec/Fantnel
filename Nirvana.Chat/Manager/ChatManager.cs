using Codexus.Development.SDK.Event;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Utils;
using Nirvana.Chat.Message;

namespace Nirvana.Chat.Manager;

public static class ChatManager {
    public static void Register()
    {
        foreach (var channel in MessageChannels.AllVersions) {
            EventManager.Instance.RegisterHandler<EventLoginSuccess>(channel, OnLoginSuccess);
        }

        EventManager.Instance.RegisterHandler<EventConnectionClosed>(MessageChannels.ChannelConnection, OnConnectionClosed);
    }

    private static void OnLoginSuccess(EventLoginSuccess args)
    {
        // 登录成功
        _ = ChatMessage.StartAsync(args.Connection);
    }

    private static void OnConnectionClosed(EventConnectionClosed args)
    {
        // 连接关闭
        _ = ChatMessage.RemoveJoin(args.Connection);
    }
}