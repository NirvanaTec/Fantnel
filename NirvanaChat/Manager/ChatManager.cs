using Codexus.Development.SDK.Connection;
using Codexus.Development.SDK.Event;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Utils;
using NirvanaChat.Message;

namespace NirvanaChat.Manager;

public static class ChatManager {
    private static bool _status; // 是否注册
    public static readonly List<GameConnection> List = [];

    public static void Register()
    {
        if (_status) {
            return;
        }

        _status = true;
        foreach (var channel in MessageChannels.AllVersions) {
            EventManager.Instance.RegisterHandler<EventLoginSuccess>(channel, OnLoginSuccess);
        }

        EventManager.Instance.RegisterHandler<EventConnectionClosed>("channel_connection", OnConnectionClosed);
    }

    private static void OnLoginSuccess(EventLoginSuccess args)
    {
        // 已存在
        if (List.Any(channel => Equals(channel, args.Connection))) {
            return;
        }

        List.Add(args.Connection);
        if (List.Count == 1) {
            ChatMessage.Start();
        }
    }

    private static void OnConnectionClosed(EventConnectionClosed args)
    {
        foreach (var channel in List.Where(channel => channel.Equals(args.Connection))) {
            List.Remove(channel);
            break;
        }

        if (List.Count == 0) {
            ChatMessage.Shutdown();
        }

        ChatMessage.RemoveJoin(args.Connection);
    }

    private static bool Equals(GameConnection entity, GameConnection entity1)
    {
        return entity.GameId.Equals(entity1.GameId) && entity.ForwardAddress.Equals(entity1.ForwardAddress) &&
               entity.ForwardPort.Equals(entity1.ForwardPort) && entity.NickName.Equals(entity1.NickName);
    }
}