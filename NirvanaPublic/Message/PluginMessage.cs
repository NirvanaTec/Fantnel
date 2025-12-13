using System.Reflection;
using Codexus.Development.SDK.Attributes;
using Codexus.Development.SDK.Manager;
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

    /**
     * 获取插件列表
     * @return 插件列表
     */
    public static List<EntityPluginState> GetPluginList(bool indexId = true)
    {
        var index = 0;
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
                Id = indexId ? index.ToString() : plugin.Id,
                Path = filePath
            }));
            index++;
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
        var pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);
        // 插件文件 文件路径数组
        var filesPath = Directory.GetFiles(pluginsPath);
        // 过滤后的插件文件 文件路径数组
        return filesPath.Where(filePath =>
            PluginManager.PluginExtensions.Any(filePath.EndsWith) ||
            PluginManager.PluginExtensions.Any(x => filePath.EndsWith(x + ".disable"))).ToArray();
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
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var filesPath = Directory.GetFiles(path);
        foreach (var filePath in filesPath)
        {
            // 删除 处于 待删除状态的插件文件
            if (filePath.EndsWith(".delete"))
            {
                File.Delete(filePath);
            }
        }
    }

    // 插件初始化
    public static void InitializeAuto()
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            // 自动更新插件
            PlugInstoreMessage.AutoUpdateCheck();
            // 获取目录下所有文件
            var files = Directory.GetFiles(path);
            // 获取目录下所有插件文件
            List<string> pluginFiles = [];
            pluginFiles.AddRange(files.Where(file => PluginManager.PluginExtensions.Any(file.EndsWith)));
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
            PluginManager.Instance.Plugins.Clear();

            // 自动插件 卸载插件
            PluginManager.Instance.EnsureUninstall();

            PluginManager.Instance.LoadPlugins(path);
            _loadedPluginPaths = pluginFiles;
            List<string> reloadPath = [];
            reloadPath.AddRange(from plugin in pluginFiles
                let enabled = PluginManager.Instance.Plugins.Any(pluginFile => plugin == pluginFile.Value.Path)
                where !enabled
                select plugin);
            LoadPlugins(reloadPath);
            if (count > 0) Log.Information("重新加载插件后的数量：{Count}", PluginManager.Instance.Plugins.Count);
        }
        catch (Exception e)
        {
            Log.Error("应用初始化失败：{e}", e);
        }
    }

    /**
     * 加载插件
     * @param pluginFiles 插件文件路径数组
     */
    private static void LoadPlugins(List<string> pluginFiles)
    {
        // 获取目录下所有插件文件
        foreach (var file in pluginFiles.Where(f => ((IEnumerable<string>) PluginManager.PluginExtensions).Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)).ToArray())
        {
            var (plugins, assemblies, instances) = GetPluginToPath1(file);
            for (var i = 0; i < plugins.Count; i++)
            {
                var plugin = plugins[i];
                PluginManager.Instance.Plugins.Add(plugin.Id.ToUpper(),
                    new PluginManager.PluginState(plugin.Id.ToUpper(), plugin.Name, plugin.Description, plugin.Version,
                        plugin.Author, plugin.Dependencies, file, assemblies[i], instances[i]));
            }
        }
    }

    private static EntityPluginState? GetPluginToId(string id)
    {
        return GetPluginList().FirstOrDefault(plugin => plugin.Id == id);
    }

    public static void TogglePlugin(string id)
    {
        lock (LockManager.PluginStatesLock)
        {
            var plugin = GetPluginToId(id);
            if (plugin?.Path == null) throw new Code.ErrorCodeException(Code.ErrorCode.PluginNotFound);

            if (plugin.Path.EndsWith(".disable"))
                File.Move(plugin.Path, plugin.Path[..^8]);
            else
                File.Move(plugin.Path, plugin.Path + ".disable");
        }
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
            
            // 标记为待删除状态
            // 如果删除失败，重启时候会自动删除
            File.Move(path, path + ".delete");
            if (File.Exists(path + ".delete"))
            {
                path += ".delete";
            }

            // 删除插件文件
            File.Delete(path);
        }
    }
}