namespace Codexus.Development.SDK.Manager;

public class EventManager {
    private static EventManager? _instance;

    private readonly Dictionary<Type, Dictionary<string, SortedDictionary<int, List<Delegate>>>> _eventHandlers = new();

    public static EventManager Instance => _instance ??= new EventManager();

    public void RegisterHandler<T>(string channel, EventHandler<T> handler, int priority = 0) where T : IEventArgs
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (string.IsNullOrEmpty(channel)) {
            throw new ArgumentNullException(nameof(channel));
        }

        var typeFromHandle = typeof(T);
        if (!_eventHandlers.TryGetValue(typeFromHandle, out var value)) {
            value = new Dictionary<string, SortedDictionary<int, List<Delegate>>>();
            _eventHandlers[typeFromHandle] = value;
        }

        if (!value.TryGetValue(channel, out var value2)) {
            int Comparison(int x, int y)
            {
                return y.CompareTo(x);
            }

            value2 = value[channel] = new SortedDictionary<int, List<Delegate>>(Comparer<int>.Create(Comparison));
        }

        if (!value2.TryGetValue(priority, out var value3)) {
            value3 = value2[priority] = [];
        }

        value3.Add(handler);
    }

    public bool UnregisterHandler<T>(string channel, EventHandler<T>? handler) where T : IEventArgs
    {
        if (string.IsNullOrEmpty(channel) || handler == null || !_eventHandlers.TryGetValue(typeof(T), out var value) ||
            !value.TryGetValue(channel, out var value2)) {
            return false;
        }

        foreach (var item in value2.Keys.ToList()) {
            var list = value2[item];
            if (list.Remove(handler)) {
                if (list.Count != 0) {
                    return true;
                }

                value2.Remove(item);
                if (value2.Count != 0) {
                    return true;
                }

                value.Remove(channel);
                if (value.Count == 0) {
                    _eventHandlers.Remove(typeof(T));
                }

                return true;
            }
        }

        return false;
    }

    public T TriggerEvent<T>(string channel, T args) where T : IEventArgs
    {
        return TriggerEvent([channel], args);
    }

    public T TriggerEvent<T>(string[] channels, T args) where T : IEventArgs
    {
        if (args == null) {
            throw new ArgumentNullException(nameof(args));
        }

        foreach (var key in channels) {
            if (!_eventHandlers.TryGetValue(typeof(T), out var value) ||
                !value.TryGetValue(key, out var priorityDict)) {
                continue;
            }

            foreach (var @delegate in priorityDict.Keys.Select(priority => priorityDict[priority])
                         .SelectMany(handlers => handlers.ToList())) {
                var item = (EventHandler<T>)@delegate;
                item(args);
            }
        }

        return args;
    }

    public bool HasHandlersForEvent<T>(string channel) where T : IEventArgs
    {
        if (string.IsNullOrEmpty(channel)) {
            return false;
        }

        if (_eventHandlers.TryGetValue(typeof(T), out var value) && value.TryGetValue(channel, out var value2)) {
            return value2.Values.Any(list => list.Count > 0);
        }

        return false;
    }

    public void ClearAllHandlers()
    {
        _eventHandlers.Clear();
    }

    public void ClearHandlersForEventType<T>() where T : IEventArgs
    {
        _eventHandlers.Remove(typeof(T));
    }

    public void ClearHandlersForEvent<T>(string channel) where T : IEventArgs
    {
        if (!string.IsNullOrEmpty(channel) && _eventHandlers.TryGetValue(typeof(T), out var value) &&
            value.Remove(channel) && value.Count == 0) {
            _eventHandlers.Remove(typeof(T));
        }
    }
}