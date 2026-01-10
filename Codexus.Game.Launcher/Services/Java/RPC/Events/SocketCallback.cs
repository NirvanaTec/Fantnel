using System;
using System.Collections.Generic;

namespace Codexus.Game.Launcher.Services.Java.RPC.Events;

public class SocketCallback
{
    private readonly Dictionary<ushort, Action<byte[]>> _receiveCallbacks = new();

    public Action<string> LostConnectCallback { get; set; }

    public void RegisterReceiveCallback(ushort sid, Action<byte[]> callback)
    {
        _receiveCallbacks[sid] = callback;
    }

    public bool InvokeCallback(ushort sid, byte[] parameters)
    {
        if (!_receiveCallbacks.TryGetValue(sid, out var value)) return false;
        value(parameters);
        return true;
    }
}