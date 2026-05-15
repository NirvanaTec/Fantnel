using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Nirvana.DevPlugin.Entities;
using Nirvana.DevPlugin.Plugins;
using Nirvana.WPFLauncher.Utils;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Development.Manager;

public static class PluginManager {
    // 插件状态 锁
    private static readonly Lock PluginStatesLock = new();

    // ID[插件]
    private static readonly Dictionary<string, PluginState> Plugins = [];

    public static void LoadPlugins()
    {
        LoadPlugins(PathUtil.PluginsPath);
    }

    private static void LoadPlugins(string directory)
    {
        Directory.CreateDirectory(directory);
        var pluginStateList = new List<PluginState>();
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filePath in Directory.GetFiles(directory, "*.dll")) {
            // 根据 Md5 判断是否需要重新加载
            var md5 = File.ReadAllBytes(filePath).EncodeMd5(); // 当前文件md5
            var flag = Plugins.Values.All(pluginState => pluginState.Md5 != md5);
            if (flag) {
                var pluginState = LoadPlugin(filePath, md5);
                if (pluginState == null) {
                    continue;
                }

                pluginStateList.Add(pluginState);
            }
        }

        CheckDependencies();
        InitializePlugins(pluginStateList);
    }

    private static PluginState? LoadPlugin(string filePath, string md5)
    {
        try {
            var assembly = Assembly.LoadFile(filePath);

            foreach (var item in PluginState.CreateAttribute<Plugin, IPlugin>(assembly)) {
                try {
                    var pluginState = new PluginState {
                        Md5 = md5,
                        Path = filePath,
                        Info = item.Key,
                        Plugin = item.Value,
                        Assembly = assembly
                    };

                    Plugins[item.Key.Id] = pluginState;
                    return pluginState;
                } catch (MissingMemberException) {
                    Log.Warning("Plugin {0} is missing plugin attribute", filePath);
                }
            }
        } catch (Exception exception) {
            Log.Error(exception, "Failed Load Plugin: {0}", filePath);
            DeletePluginByPath(filePath);
        }

        return null;
    }

    /**
     * 获取依赖插件
     * @param id 插件ID
     * @return 依赖插件列表
     */
    private static List<PluginState> GetDependenciesById(string id)
    {
        var pluginList = new List<PluginState>();
        foreach (var plugin in Plugins.Values) {
            if (plugin.Info.Dependencies == null) {
                continue;
            }

            if (plugin.Info.Dependencies.Any(dependency => dependency == id)) {
                pluginList.Add(plugin);
            }
        }

        return pluginList;
    }

    /**
     * 删除插件
     * @param id 插件ID
     */
    public static void DeletePlugin(string id)
    {
        lock (PluginStatesLock) {
            LoadPlugins();
            foreach (var plugin in GetDependenciesById(id)) {
                DeletePluginByPath(plugin.Path);
            }

            DeletePluginByPath(GetPluginById(id)?.Path);
        }
    }

    /**
     * 删除插件
     * @param path 插件路径
     */
    private static void DeletePluginByPath(string? path)
    {
        if (path == null) {
            return;
        }

        // 插件路径
        var path1 = path + "." + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".delete";
        // 标记为待删除状态
        // 重启时会自动删除
        File.Move(path, path1);
        Log.Warning("插件 {0} 已标记为待删除状态", Path.GetFileName(path));
    }

    public static void DeletePlugin()
    {
        DeletePluginExecute(PathUtil.PluginsPath);
    }

    private static void DeletePluginExecute(string path)
    {
        Directory.CreateDirectory(path);
        foreach (var filePath in Directory.GetFiles(path)) {
            // 删除 处于 待删除状态的插件文件
            if (filePath.EndsWith(".delete")) {
                try {
                    File.Delete(filePath);
                } catch (Exception e) {
                    Log.Warning("删除插件: {0} 失败: {1}", filePath, e.Message);
                }
            }
        }
    }

    /**
     * 切换插件状态
     * @param id 插件ID
     * -1:自动切换
     * 0:禁用
     * 1:启用
     */
    public static void TogglePlugin(string id, int auto = -1)
    {
        lock (PluginStatesLock) {
            var plugin = GetPluginById(id);
            if (plugin?.Path == null) {
                throw new ErrorCodeException(ErrorCode.PluginNotFound);
            }

            switch (auto) {
                case -1: {
                    if (plugin.Path.EndsWith(".disable")) {
                        File.Move(plugin.Path, plugin.Path[..^8]);
                    } else {
                        DisablePlugin(plugin);
                    }

                    break;
                }
                case 0: {
                    DisablePlugin(plugin);
                    break;
                }
                case 1: {
                    if (plugin.Path.EndsWith(".disable")) {
                        File.Move(plugin.Path, plugin.Path[..^8]);
                    }

                    break;
                }
            }
        }
    }

    /**
     * 禁用插件
     */
    private static void DisablePlugin(PluginState pluginState)
    {
        lock (PluginStatesLock) {
            LoadPlugins();
            foreach (var plugin in GetDependenciesById(pluginState.Info.Id)) {
                DisablePluginByPath(plugin.Path);
            }

            DisablePluginByPath(pluginState.Path);
        }
    }

    /**
     * 禁用插件
     */
    private static void DisablePluginByPath(string? path)
    {
        if (path == null) {
            return;
        }

        if (!path.EndsWith(".disable")) {
            File.Move(path, path + ".disable");
        }
    }

    /**
     * 检查插件依赖
     */
    private static void CheckDependencies()
    {
        foreach (var value in Plugins.Values) {
            var dependencies = value.Info.Dependencies;
            if (dependencies == null) {
                continue;
            }

            foreach (var text in dependencies) {
                if (Plugins.ContainsKey(text)) {
                    continue;
                }

                throw new InvalidOperationException($"Plugin {value.Info.Name}({value.Info.Id}) depends on {text}, but it is not loaded");
            }
        }
    }

    /**
     * 初始化插件
     */
    private static void InitializePlugins(List<PluginState> pluginStateList)
    {
        foreach (var pluginState in pluginStateList) {
            pluginState.Plugin.OnInitialize();
        }
    }

    /**
     * 创建插件属性
     */
    public static Dictionary<TKey, TValue> CreateAttribute<TKey, TValue>() where TKey : Attribute
    {
        return Plugins.Values.Select(plugin => plugin.CreateAttribute<TKey, TValue>()).SelectMany(attributes => attributes).ToDictionary(attributeListItem => attributeListItem.Key, attributeListItem => attributeListItem.Value);
    }

    /**
     * 获取插件
     * @param id 插件ID
     */
    public static PluginState? GetPluginById(string id)
    {
        LoadPlugins();
        return Plugins.Values.FirstOrDefault(plugin => plugin.Info.Id == id);
    }

    /**
     * 获取所有插件状态
     */
    public static EntityPluginState[] GetPluginStates()
    {
        LoadPlugins();
        return Plugins.Values.Select(plugin => plugin.ToPluginStates()).ToArray();
    }

    /**
     * @return 插件路径/MD5
     */
    public static Dictionary<string, string> GetPluginPathAndMd5()
    {
        LoadPlugins();
        return Plugins.Values.ToDictionary(plugin => plugin.Path, plugin => plugin.Md5);
    }
}