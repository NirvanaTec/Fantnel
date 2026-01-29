using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Message;

public static class ServersGameMessage
{
    // 服务器列表[普通信息] - 缓存
    private static readonly List<EntityNetGameItem> ServerList = [];

    /**
     * 获取服务器列表[普通信息]
     * @param offset 偏移量
     * @param pageSize 每页数量
     * @param safeImage 是否安全获取图片
     * @return 服务器列表[普通信息]
     */
    public static async Task<EntityNetGameItem[]> GetServerList(int offset = 0, int pageSize = 10, bool safeImage = true)
    {
        var index = 0;
        while (true)
        {
            // ServerList 有 就用缓存
            // 分页
            var size = pageSize + offset;
            if (ServerList.Count >= size)
            {
                var list = ServerList.Skip(offset).Take(pageSize).ToArray();
                if (!safeImage)
                {
                    return list;
                }
                // 修复没有图片的游戏项
                foreach (var item in list)
                {
                    // 没有图片
                    if (item.TitleImageUrl.Contains("http"))
                    {
                        continue;
                    }
                    // 从 详情页 获取图片
                    item.TitleImageUrl = GetFirstImage(item.EntityId).Result;
                }

                return list;
            }
            if (++index > 1)
            {
                // 最后一页, 减少数量，避免丢失数据
                pageSize--;
            }
            else
            {
                var items = await WPFLauncher.GetAvailableNetGamesAsync(ServerList.Count, size);
                AddServerList(items);
            }
        }
    }

    // 获取 第一张 图片
    private static async Task<string> GetFirstImage(string id)
    {
        var details = await WPFLauncher.GetNetGameDetailByIdAsync(id);
        // if (details != null && details.BriefImageUrls.Length > 0)
        return details is { BriefImageUrls.Length: > 0 } ? details.BriefImageUrls[0] : "";
    }

    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityNetGameItem gameItem)
    {
        foreach (var item in ServerList.Where(item => item.EntityId == gameItem.EntityId))
        {
            if (item.TitleImageUrl == "" && gameItem.TitleImageUrl != "")
                item.TitleImageUrl = gameItem.TitleImageUrl;
            return;
        }
        ServerList.Add(gameItem);
    }
    
    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityNetGameItem[] gameItems)
    {
        foreach (var item in gameItems)
            AddServerList(item);
    }

    /**
     * 服务器列表[普通信息] - 获取
     * @param id 服务器ID
     * @return 服务器信息
     */
    public static EntityNetGameItem? GetServerById(string id)
    {
        for (var i = 0; i < 100; i++)
        {
            var server = ServerList.Find(server => server.EntityId == id);
            if (server != null) return server;
            GetServerList(10 * i, 10, false).Wait();
        }

        return null;
    }
    
    /**
    * 获取服务器上的指定游戏角色
    * @param serverId 服务器ID
    * @param name 游戏角色名称
    * @return 服务器上的指定游戏角色
    */
    public static async Task<EntityGameCharacter> GetUserName(string serverId, string name)
    {
        for (var i = 0; i < 3; i++)
        {
            try
            {
                var games = await WPFLauncher.GetNetGameCharactersAsync(serverId);
                if (games == null) throw new ErrorCodeException(ErrorCode.NotFound);
                foreach (var game in games)
                    if (game.Name == name)
                        return game;
            }
            catch (Exception e)
            {
                Log.Error("获取游戏角色 {0} 失败 {1}", name, e.Message);
            }

            Thread.Sleep(800);
        }

        throw new ErrorCodeException(ErrorCode.NotFoundName);
    }
    
}