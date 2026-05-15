using System;
using System.Collections.Generic;
using System.Reflection;
using Nirvana.DevPlugin.Entities;
using Serilog;

namespace Nirvana.DevPlugin.Plugins;

public class PluginState {
    public required Assembly Assembly;
    public required Plugin Info;
    public required string Md5;
    public required string Path;
    public required IPlugin Plugin;

    private string IsEnabled => Path.EndsWith(".disable") ? "0" : "1";

    private static Type[] GetTypes(Assembly tagetAssembly, Type tagetType)
    {
        var types = new List<Type>();
        foreach (var type in tagetAssembly.GetTypes()) {
            if (tagetType.IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false }) {
                types.Add(type);
            }
        }

        return types.ToArray();
    }

    private static Dictionary<Type, T> CreateInstance<T>(Assembly tagetAssembly)
    {
        var instances = new Dictionary<Type, T>();
        foreach (var item in GetTypes(tagetAssembly, typeof(T))) {
            if (Activator.CreateInstance(item) is not T t) {
                Log.Warning("Plugin {0} is not a {1}", item.FullName, typeof(T).Name);
                continue;
            }

            instances.Add(item, t);
        }

        return instances;
    }

    public Dictionary<TKey, TValue> CreateAttribute<TKey, TValue>() where TKey : Attribute
    {
        return CreateAttribute<TKey, TValue>(Assembly);
    }

    public static Dictionary<TKey, TValue> CreateAttribute<TKey, TValue>(Assembly tagetAssembly) where TKey : Attribute
    {
        var instances = new Dictionary<TKey, TValue>();
        foreach (var item in CreateInstance<TValue>(tagetAssembly)) {
            var key = item.Key.GetCustomAttribute<TKey>(false);
            if (key == null) {
                Log.Warning("Plugin {0} is missing {1} attribute", item.Key.FullName, typeof(TKey).Name);
                continue;
            }

            instances.Add(key, item.Value);
        }

        return instances;
    }

    public EntityPluginState ToPluginStates()
    {
        return new EntityPluginState {
            Name = Info.Name,
            Version = Info.Version,
            Author = Info.Author,
            Status = IsEnabled,
            Id = Info.Id,
            Path = Path
        };
    }
}