using System;
using System.Collections.Generic;
using System.Linq;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Events;

namespace Nirvana.Development.Manager;

public static class EventManager {
    // private static readonly List<object> BaseEvents = [];

    public static void TriggerEvent<T>(Action<T> onEvent, EnumProtocolVersion protocolVersion)
    {
        var events = TriggerEvent<T>(protocolVersion);
        foreach (var item in events) {
            onEvent(item);
        }
    }

    public static T? TriggerEvent<T>(Func<T, bool> onEvent, EnumProtocolVersion protocolVersion)
    {
        var events = TriggerEvent<T>(protocolVersion);
        return events.FirstOrDefault(onEvent);
    }

    private static T[] TriggerEvent<T>(EnumProtocolVersion protocolVersion)
    {
        var events = new List<T>();
        // foreach (var item in BaseEvents) {
        //     if (item is T t) {
        //         events.Add(t);
        //     }
        // }

        events.AddRange(PluginManager.CreateAttribute<RegisterEvent, T>().Where(item => item.Key.Version == EnumProtocolVersion.All || item.Key.Version == protocolVersion).Select(item => item.Value));
        return events.ToArray();
    }
}