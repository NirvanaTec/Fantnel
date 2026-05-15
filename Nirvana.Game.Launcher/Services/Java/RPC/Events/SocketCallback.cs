using System;
using System.Collections.Generic;

namespace Nirvana.Game.Launcher.Services.Java.RPC.Events;

public class SocketCallback {
    private readonly Dictionary<ushort, Action<byte[]>> _receiveCallbacks = new();

    public void RegisterReceiveCallback(ushort sid, Action<byte[]> callback)
    {
        _receiveCallbacks[sid] = callback;
    }

    public void InvokeCallback(ushort sid, byte[] parameters)
    {
        if (!_receiveCallbacks.TryGetValue(sid, out var value)) {
            return;
        }
        value(parameters);
    }
}