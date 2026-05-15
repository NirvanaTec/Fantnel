using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nirvana.Development.Manager;
using Nirvana.Public.Entities.Nirvana;
using Nirvana.WPFLauncher.Http;
using NirvanaAPI.Entities;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Public.Message;

public static class PluginMessage {
    /**
     * 插件初始化
     */
    public static void Initialize()
    {
        PluginManager.DeletePlugin();
        PlugInstoreMessage.AutoUpdateCheck(); // 自动更新插件
    }

    /**
     * 插件初始化
     */
    public static void InitializeAuto()
    {
        try {
            PlugInstoreMessage.AutoUpdateCheck(); // 自动更新插件
            PluginManager.LoadPlugins(); // 加载插件
            // ChatPluginMain.Initialize(); // 初始化 N聊天插件
        } catch (Exception e) {
            Log.Error("应用初始化失败：{0}", e);
        }
    }

    /**
    * 获取服务器依赖列表
    */
    public static List<EntityDependence> GetDependenceList(string? id, string? version)
    {
        return GetDependenceListAsync(id, version).GetAwaiter().GetResult();
    }

    /**
    * 获取服务器依赖列表
    */
    private static async Task<List<EntityDependence>> GetDependenceListAsync(string? id, string? version)
    {
        var dependenceList = await GetDependenceListNetAsync(id, version);
        if (dependenceList.Count == 0) {
            throw new ErrorCodeException(ErrorCode.GamePlugin);
        }

        var list = new List<EntityDependence>();
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var entity1 in dependenceList) {
            // data
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entity2 in entity1.Data) {
                // data > id
                if (!IsPluginExist(entity2.Id)) {
                    list.Add(entity1);
                }
            }
        }

        return list;
    }

    /**
     * 获取服务器依赖列表
     */
    private static async Task<List<EntityDependence>> GetDependenceListNetAsync(string? id, string? version)
    {
        var entity = await X19Extensions.Nirvana.Api<EntityResponse<List<EntityDependence>>>("/api/fantnel/dependence?id=" + (id ?? "") + "&version=" + (version ?? ""));
        return entity?.Data ?? throw new ErrorCodeException(ErrorCode.NotFound);
    }

    /**
     * 插件是否存在
     */
    private static bool IsPluginExist(string id)
    {
        return PluginManager.GetPluginById(id) != null;
    }
}