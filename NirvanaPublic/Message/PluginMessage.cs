using System.Reflection;
using Codexus.Development.SDK.Attributes;
using Codexus.Development.SDK.Enums;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Packet;
using Codexus.Development.SDK.Plugin;
using Codexus.Interceptors;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;
using Serilog;

namespace NirvanaPublic.Message;

public static class PluginMessage
{
    // 已经加载的插件路径
    private static List<string> _loadedPluginPaths = [];

    private static readonly string[] PluginExtensions = [".ug", ".dll"];

    /**
     * 获取插件列表
     * @return 插件列表
     */
    public static List<EntityPluginState> GetPluginList()
    {
        // 插件文件 文件路径数组
        var filesPath = GetPluginDirectoryList();
        // 插件状态 数组
        var pluginStates = new List<EntityPluginState>();
        foreach (var filePath in filesPath)
        {
            // 获取插件信息
            var plugins = GetPluginToPath(filePath);
            pluginStates.AddRange(plugins.Select(plugin => new EntityPluginState
            {
                Name = plugin.Name,
                Version = plugin.Version,
                Author = plugin.Author,
                Status = filePath.EndsWith(".disable") ? "0" : "1",
                Id = plugin.Id,
                Path = filePath,
                Dependencies = plugin.Dependencies
            }));
        }

        return pluginStates;
    }

    /**
     * 获取插件目录列表
     * @return 插件目录列表
     */
    private static string[] GetPluginDirectoryList()
    {
        // 插件目录 文件夹
        var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);
        // 插件文件 文件路径数组
        var filesPath = Directory.GetFiles(pluginsPath);
        // 过滤后的插件文件 文件路径数组
        return filesPath.Where(filePath =>
            PluginExtensions.Any(filePath.EndsWith) ||
            PluginExtensions.Any(x => filePath.EndsWith(x + ".disable"))).ToArray();
    }

    /**
     * 获取插件列表
     * @return 1.插件目录数组 2.插件md5数组
     */
    public static (string[], string[]) GetPluginDirectoryAndMd5List()
    {
        // 插件文件 文件路径数组
        var filesPath = GetPluginDirectoryList();
        // 插件目录和md5 数组
        var pluginMd5List = filesPath.Select(Tools.GetFileMd5).ToList();
        return (filesPath, pluginMd5List.ToArray());
    }

    /**
     * 获取插件信息
     * @param path 插件文件路径
     * @return 插件信息数组
     */
    private static (List<Plugin>, List<Assembly>, List<IPlugin>) GetPluginToPath1(string path)
    {
        List<Plugin> plugins = [];
        List<Assembly> assemblies = [];
        List<IPlugin> instances = [];
        try
        {
            var assembly = Assembly.LoadFrom(path);
            foreach (var type in assembly.GetTypes().Where((Func<Type, bool>)(type =>
                         typeof(IPlugin).IsAssignableFrom(type) &&
                         type is { IsAbstract: false, IsInterface: false })))
            {
                Plugin? customAttribute;
                try
                {
                    customAttribute = type.GetCustomAttribute<Plugin>(false);
                }
                catch (MissingMemberException)
                {
                    Log.Warning("插件 {TypeFullName} 没有插件属性", type.FullName);
                    continue;
                }

                if (customAttribute == null)
                {
                    Log.Warning("插件 {TypeFullName} 没有插件属性", type.FullName);
                    continue;
                }

                if (Activator.CreateInstance(type) is not IPlugin instance)
                {
                    Log.Warning("插件 {TypeFullName} 没有继承 IPlugin", type.FullName);
                    continue;
                }

                assemblies.Add(assembly);
                instances.Add(instance);
                plugins.Add(customAttribute);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load plugin from {File}", path);
        }

        return (plugins, assemblies, instances);
    }

    private static Plugin[] GetPluginToPath(string path)
    {
        return GetPluginToPath1(path).Item1.ToArray();
    }

    // 插件初始化
    public static void Initialize()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var filesPath = Directory.GetFiles(path);
        foreach (var filePath in filesPath)
            // 删除 处于 待删除状态的插件文件
            if (filePath.EndsWith(".delete"))
                File.Delete(filePath);
    }

    // 插件初始化
    public static void InitializeAuto()
    {
        try
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            // 自动更新插件
            PlugInstoreMessage.AutoUpdateCheck();
            // 获取目录下所有文件
            var files = Directory.GetFiles(path);
            // 获取目录下所有插件文件
            List<string> pluginFiles = [];
            pluginFiles.AddRange(files.Where(file => PluginExtensions.Any(file.EndsWith)));
            // 比较插件文件是否变动
            // pluginFiles 和 LoadedPluginPaths
            // 内容是否完全一致
            if (pluginFiles.SequenceEqual(_loadedPluginPaths) || _loadedPluginPaths.SequenceEqual(pluginFiles)) return;
            var count = _loadedPluginPaths.Count;
            if (count > 0) Log.Information("插件文件变动，重新加载插件");
            // Interceptor 确保加载
            Interceptor.EnsureLoaded();
            // PacketManager 确保注册
            PacketManager.Instance.EnsureRegistered();
            // 清空插件以便重新加载
            ClearPlugin();
            // 自动插件 卸载插件
            PluginManager.Instance.EnsureUninstall();
            PluginManager.Instance.LoadPlugins(path);

            var initializeMethod = typeof(PluginManager).GetMethod("InitializePlugins",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (initializeMethod != null)
                initializeMethod.Invoke(PluginManager.Instance, null);
            else
                Log.Fatal("没有 InitializePlugins 方法, 这可能导致插件加载失败");

            _loadedPluginPaths = pluginFiles;
            if (count > 0) Log.Information("重新加载插件后的数量：{Count}", PluginManager.Instance.Plugins.Count);
        }
        catch (Exception e)
        {
            Log.Error("应用初始化失败：{e}", e);
        }
    }

    /**
     * 清空插件状态
     */
    private static void ClearPlugin()
    {
        PluginManager.Instance.Plugins.Clear();
        // PluginManager.Instance._loadedFiles
        var loadedFilesField = typeof(PluginManager).GetField("_loadedFiles",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (loadedFilesField == null)
            Log.Fatal("没有 _loadedFiles 属性, 这可能导致插件加载失败");
        else
            loadedFilesField.SetValue(PluginManager.Instance, new HashSet<string>());
        // PacketManager.Instance._ids
        var idsField = typeof(PacketManager).GetField("_ids",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (idsField == null)
        {
            Log.Fatal("没有 _ids 属性, 这可能导致插件加载失败");
        }
        else
        {
            var idsObj = idsField.GetValue(PacketManager.Instance);
            var idsDict = idsObj as Dictionary<Type, Dictionary<EnumProtocolVersion, int>>;
            var idList = new Dictionary<Type, Dictionary<EnumProtocolVersion, int>>();
            if (idsDict != null)
                foreach (var id in idsDict)
                {
                    var name = id.Key.AssemblyQualifiedName;
                    if (name == null) continue;

                    if (name.StartsWith("Codexus.Interceptors.Packet.")) idList.Add(id.Key, id.Value);
                }

            idsField.SetValue(PacketManager.Instance, idList);
        }

        // PacketManager.Instance._metadata
        var metadataField = typeof(PacketManager).GetField("_metadata",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (metadataField == null)
            Log.Fatal("没有 _metadata 属性, 这可能导致插件加载失败");
        else
            metadataField.SetValue(PacketManager.Instance, new Dictionary<Type, RegisterPacket>());
        // PacketManager.Instance._packets
        // Codexus 这写法....哎。不讲、不讲。
        var packetsField = typeof(PacketManager).GetField("_packets",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (packetsField == null)
        {
            Log.Fatal("没有 _packets 属性, 这可能导致插件加载失败");
        }
        else
        {
            var packetsObj = packetsField.GetValue(PacketManager.Instance);
            if (packetsObj is Dictionary<EnumConnectionState,
                    Dictionary<EnumPacketDirection, Dictionary<EnumProtocolVersion, Dictionary<int, Type>>>>
                packetsDict)
            {
                var newPacket =
                    new Dictionary<EnumConnectionState, Dictionary<EnumPacketDirection,
                        Dictionary<EnumProtocolVersion, Dictionary<int, Type>>>>();
                foreach (var packet in packetsDict)
                {
                    var packet1 = packet.Value;
                    var newPacket3 =
                        new Dictionary<EnumPacketDirection, Dictionary<EnumProtocolVersion, Dictionary<int, Type>>>();
                    foreach (var packet2 in packet1)
                    {
                        var packet3 = packet2.Value;
                        var newPacket5 = new Dictionary<EnumProtocolVersion, Dictionary<int, Type>>();
                        foreach (var packet4 in packet3)
                        {
                            var packet5 = packet4.Value;
                            var newPacket6 = new Dictionary<int, Type>();
                            foreach (var packet6 in packet5)
                            {
                                var packet7 = packet6.Value;
                                var name = packet7.AssemblyQualifiedName;
                                if (name == null) continue;
                                if (name.StartsWith("Codexus.Interceptors.Packet."))
                                    newPacket6.Add(packet6.Key, packet6.Value);
                            }

                            newPacket5.Add(packet4.Key, newPacket6);
                        }

                        newPacket3.Add(packet2.Key, newPacket5);
                    }

                    newPacket.Add(packet.Key, newPacket3);
                }

                packetsField.SetValue(PacketManager.Instance, newPacket);
            }
        }

        // PacketManager.Instance._states
        var statesField = typeof(PacketManager).GetField("_states",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (statesField == null)
        {
            Log.Fatal("没有 _states 属性, 这可能导致插件加载失败");
        }
        else
        {
            var statesObj = statesField.GetValue(PacketManager.Instance);
            var statesDict = statesObj as Dictionary<Type, EnumConnectionState>;
            var states = new Dictionary<Type, EnumConnectionState>();
            if (statesDict != null)
                foreach (var state in statesDict)
                {
                    var name = state.Key.AssemblyQualifiedName;
                    if (name == null) continue;

                    if (name.StartsWith("Codexus.Interceptors.Packet.")) states.Add(state.Key, state.Value);
                }

            statesField.SetValue(PacketManager.Instance, states);
        }
    }

    private static EntityPluginState? GetPluginToId(string id)
    {
        return GetPluginList().FirstOrDefault(plugin => plugin.Id == id);
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
        lock (LockManager.PluginStatesLock)
        {
            var plugin = GetPluginToId(id);
            if (plugin?.Path == null) throw new Code.ErrorCodeException(Code.ErrorCode.PluginNotFound);
            switch (auto)
            {
                case -1:
                {
                    if (plugin.Path.EndsWith(".disable"))
                    {
                        File.Move(plugin.Path, plugin.Path[..^8]);
                    }
                    else
                    {
                        File.Move(plugin.Path, plugin.Path + ".disable");
                        DependenciesPlugin(id);
                    }

                    break;
                }
                case 0:
                {
                    if (!plugin.Path.EndsWith(".disable"))
                    {
                        File.Move(plugin.Path, plugin.Path + ".disable");
                        DependenciesPlugin(id);
                    }

                    break;
                }
                case 1:
                {
                    if (plugin.Path.EndsWith(".disable"))
                        File.Move(plugin.Path, plugin.Path[..^8]);
                    break;
                }
            }
        }
    }

    // 依赖禁用
    private static List<string> DependenciesPlugin(string id)
    {
        var pluginList = new List<string>();
        var plugins = GetPluginList();
        foreach (var plugin in plugins)
        {
            if (plugin.Dependencies == null) continue;
            if (plugin.Dependencies.All(dependency => dependency != id)) continue;
            TogglePlugin(plugin.Id, 0);
            pluginList.Add(plugin.Id);
        }

        return pluginList;
    }

    // 删除插件
    public static void DeletePlugin(string id)
    {
        lock (LockManager.PluginStatesLock)
        {
            var plugin = GetPluginToId(id);
            if (plugin?.Path == null) throw new Code.ErrorCodeException(Code.ErrorCode.PluginNotFound);

            // 插件路径
            var path = plugin.Path;
            var path1 = path + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".delete";

            // 标记为待删除状态
            // 如果删除失败，重启时候会自动删除
            File.Move(path, path1);
            if (File.Exists(path1)) path = path1;

            try
            {
                // 删除插件文件
                File.Delete(path);
            }
            catch (Exception)
            {
                Log.Warning("插件 {name} 已标记为待删除状态", plugin.Name);
            }
        }

        // 删除依赖插件
        foreach (var pluginId in DependenciesPlugin(id)) DeletePlugin(pluginId);
    }
}