using System.Reflection;
using Codexus.Development.SDK.Attributes;
using Codexus.Development.SDK.Manager;
using Codexus.Development.SDK.Plugin;
using Codexus.Interceptors;
using NirvanaAPI.Entities;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using NirvanaChat.Manager;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Entities.Nirvana;
using NirvanaPublic.Manager;
using Serilog;
using WPFLauncherApi.Http;

namespace NirvanaPublic.Message;

public static class PluginMessage {
    private static readonly string[] PluginExtensions = [".ug", ".dll"];

    /**
     * 获取插件列表
     * @return 插件列表
     */
    public static List<EntityPluginState> GetPluginListSafe()
    {
        CleanSameIdPlugin();
        return GetPluginList();
    }

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
        foreach (var filePath in filesPath) {
            // 获取插件信息
            var plugins = GetPluginToPath(filePath);
            pluginStates.AddRange(plugins.Select(plugin => new EntityPluginState {
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
        var pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        Directory.CreateDirectory(pluginsPath);
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
    private static (List<Plugin>, List<Assembly>, List<IPlugin>) GetPluginToPath1(string pluginPath)
    {
        List<Plugin> plugins = [];
        List<Assembly> assemblies = [];
        List<IPlugin> instances = [];
        try {
            // 检查是否已经加载了相同名称的程序集
            var assemblyName = AssemblyName.GetAssemblyName(pluginPath);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName.Name &&
                                     a.GetName().Version == assemblyName.Version);
            if (assembly == null)
                // 如果未加载，从路径加载
                assembly = Assembly.LoadFrom(pluginPath);
            foreach (var type in assembly.GetTypes().Where((Func<Type, bool>)(type =>
                         typeof(IPlugin).IsAssignableFrom(type) &&
                         type is { IsAbstract: false, IsInterface: false }))) {
                Plugin? customAttribute;
                try {
                    customAttribute = type.GetCustomAttribute<Plugin>(false);
                } catch (MissingMemberException) {
                    Log.Warning("插件 {TypeFullName} 没有插件属性", type.FullName);
                    continue;
                }

                if (customAttribute == null) {
                    Log.Warning("插件 {TypeFullName} 没有插件属性", type.FullName);
                    continue;
                }

                if (Activator.CreateInstance(type) is not IPlugin instance) {
                    Log.Warning("插件 {TypeFullName} 没有继承 IPlugin", type.FullName);
                    continue;
                }

                assemblies.Add(assembly);
                instances.Add(instance);
                plugins.Add(customAttribute);
            }
        } catch (Exception ex) {
            Log.Error("无法加载插件: {Message}", ex.Message);
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
        Directory.CreateDirectory(path);
        var filesPath = Directory.GetFiles(path);
        foreach (var filePath in filesPath) {
            // 删除 处于 待删除状态的插件文件
            if (!filePath.EndsWith(".delete")) continue;
            try {
                File.Delete(filePath);
            } catch (Exception e) {
                Log.Warning("删除插件: {FilePath} 失败: {Exception}", filePath, e.Message);
            }
        }
    }

    // 清理相同ID的插件
    // 保留高版本插件, 清理低版本插件
    // 相同插件：根据修改日期，再根据短名称
    public static void CleanSameIdPlugin()
    {
        var plugins = GetPluginList();
        foreach (var plugin in plugins.ToList()) {
            if (plugin.Version == null || plugin.Path == null) continue;

            var verA = new Version(plugin.Version);
            foreach (var plugin1 in plugins.ToList()) {
                if (plugin1.Version == null || plugin1.Path == null) continue;

                var verB = new Version(plugin1.Version);
                var verCompare = verA.CompareTo(verB);
                if (plugin.Id != plugin1.Id || plugin == plugin1 || verCompare > 0) continue;
                var plugin1Name = Path.GetFileName(plugin1.Path);
                if (verCompare < -1) {
                    Log.Warning("清理旧插件: {FilePath}", plugin1Name);
                    DeletePlugin(plugin1);
                    plugin1.Path = null;
                } else {
                    // 根据修改日期 清理, 保留最新插件
                    var pluginFile = new FileInfo(plugin.Path);
                    var plugin1File = new FileInfo(plugin1.Path);
                    if (pluginFile.LastWriteTime > plugin1File.LastWriteTime) {
                        Log.Warning("清理同名插件: {FilePath}", plugin1Name);
                        DeletePlugin(plugin1);
                        plugin1.Path = null;
                    } else if (plugin1File.LastWriteTime > pluginFile.LastWriteTime) {
                        DeletePlugin(plugin);
                        plugin.Path = null;
                        break;
                    } else {
                        // 根据名称长度 清理, 保留短名插件
                        var pluginName = Path.GetFileName(plugin.Path);
                        if (plugin1Name.Length > pluginName.Length) {
                            Log.Warning("清理同名插件: {FilePath}", plugin1Name);
                            DeletePlugin(plugin1);
                            plugin1.Path = null;
                        } else {
                            DeletePlugin(plugin);
                            plugin.Path = null;
                            break;
                        }
                    }
                }
            }
        }
    }

    // 插件初始化
    public static void InitializeAuto()
    {
        try {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            Directory.CreateDirectory(path);
            PlugInstoreMessage.AutoUpdateCheck(); // 自动更新插件
            Interceptor.EnsureLoaded(); // 确保加载
            PacketManager.Instance.EnsureRegistered(); // 确保注册
            PluginManager.Instance.EnsureUninstall(); // 备用卸载
            PluginManager.Instance.LoadPlugins(path); // 加载插件
            ChatManager.Register();
        } catch (Exception e) {
            Log.Error("应用初始化失败：{e}", e);
        }
    }

    /**
     * 插件是否发生变化
     * 与 已加载完成的插件，对比是否有变化
     * 允许变多，不允许变少
     * @return 是否发生变化
     */
    public static bool IsPluginChanged()
    {
        var plugins = GetPluginList();
        return PluginManager.Instance.Plugins.Count > plugins.Count || PluginManager.Instance.Plugins.Any(plugin =>
            plugins.Where(plugin1 => plugin.Value.Id == plugin1.Id)
                .Any(plugin1 => plugin.Value.Version != plugin1.Version));
    }

    private static EntityPluginState? GetPluginToId(string id)
    {
        return GetPluginList().FirstOrDefault(plugin => plugin.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
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
        lock (LockManager.PluginStatesLock) {
            var plugin = GetPluginToId(id);
            if (plugin?.Path == null) throw new ErrorCodeException(ErrorCode.PluginNotFound);
            switch (auto) {
                case -1: {
                    if (plugin.Path.EndsWith(".disable")) {
                        File.Move(plugin.Path, plugin.Path[..^8]);
                    } else {
                        File.Move(plugin.Path, plugin.Path + ".disable");
                        DependenciesPlugin(id);
                    }

                    break;
                }
                case 0: {
                    if (!plugin.Path.EndsWith(".disable")) {
                        File.Move(plugin.Path, plugin.Path + ".disable");
                        DependenciesPlugin(id);
                    }

                    break;
                }
                case 1: {
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
        foreach (var plugin in plugins) {
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
        lock (LockManager.PluginStatesLock) {
            var plugin = GetPluginToId(id);
            if (plugin?.Path == null) {
                throw new ErrorCodeException(ErrorCode.PluginNotFound);
            }

            // 插件路径
            DeletePlugin(plugin);
        }

        // 删除依赖插件
        foreach (var pluginId in DependenciesPlugin(id)) {
            DeletePlugin(pluginId);
        }
    }

    // 删除插件
    private static void DeletePlugin(EntityPluginState plugin)
    {
        // 插件路径
        var path = plugin.Path;
        if (path == null) throw new ErrorCodeException(ErrorCode.PluginNotFound);
        var path1 = path + "." + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".delete";

        // 标记为待删除状态
        // 如果删除失败，重启时候会自动删除
        File.Move(path, path1);
        if (File.Exists(path1)) {
            path = path1;
        }

        try {
            // 删除插件文件
            File.Delete(path);
        } catch (Exception) {
            Log.Warning("插件 {name} 已标记为待删除状态", Path.GetFileName(plugin.Path));
        }
        
    }

    /**
     * 获取服务器依赖列表
     */
    public static async Task<List<EntityDependence>> GetDependenceList(string? id, string? version)
    {
        var entity = await X19Extensions.Nirvana.Api<EntityResponse<List<EntityDependence>>>(
            "/api/fantnel/dependence?id=" + (id ?? "") + "&version=" + (version ?? ""));
        return entity?.Data ?? throw new ErrorCodeException(ErrorCode.NotFound);
    }

    // 插件是否存在
    public static bool IsPluginExist(string id)
    {
        return GetPluginToId(id) != null;
    }
}