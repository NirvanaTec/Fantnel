using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;

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
    public static EntityNetGameItem[] GetServerList(int offset = 0, int pageSize = 10,
        bool safeImage = true)
    {
        // ServerList 有 就用缓存
        lock (LockManager.GameServerListLock)
        {
            // 分页 异常顺序 检测
            var size = offset + (pageSize - 10);
            if (ServerList.Count < size) GetServerList(0, size, false);
            // 分页
            size = (offset == 0 ? 1 : offset) * pageSize;
            if (ServerList.Count >= size)
            {
                var list = ServerList.Skip(size - pageSize).Take(pageSize).ToArray();
                if (!safeImage) return list;
                // 修复没有图片的游戏项
                foreach (var item in list)
                    // 没有图片
                    if (!item.TitleImageUrl.Contains("http"))
                        // 从 详情页 获取图片
                        item.TitleImageUrl = GetFirstImage(item.EntityId);

                return list;
            }
        }

        var result = InitProgram.GetServices().Wpf.GetAvailableNetGames(InfoManager.GetGameUser().UserId,
            InfoManager.GetGameUser().AccessToken, offset, pageSize);

        if (result == null) throw new Code.ErrorCodeException(Code.ErrorCode.NotFound);

        // safeImage: 没有图片的游戏项 就从 详情页 获取图片
        var items = result.Data;
        foreach (var item in items)
        {
            if (safeImage && !item.TitleImageUrl.Contains("http")) item.TitleImageUrl = GetFirstImage(item.EntityId);
            AddServerList(item);
        }

        return result.Data;
    }

    // 获取 第一张 图片
    private static string GetFirstImage(string id)
    {
        var details = ServerInfoMessage.GetServerId2(id).Result;
        // if (details != null && details.BriefImageUrls.Length > 0)
        return details is { BriefImageUrls.Length: > 0 } ? details.BriefImageUrls[0] : "";
    }

    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityNetGameItem gameItem)
    {
        lock (LockManager.GameServerListLock)
        {
            foreach (var item in ServerList.Where(item => item.EntityId == gameItem.EntityId))
            {
                if (item.TitleImageUrl == "" && gameItem.TitleImageUrl != "")
                    item.TitleImageUrl = gameItem.TitleImageUrl;
                return;
            }

            ServerList.Add(gameItem);
        }
    }

    /**
     * 服务器列表[普通信息] - 获取
     * @param id 服务器ID
     * @return 服务器信息
     */
    public static EntityNetGameItem? GetServerId(string id)
    {
        for (var i = 0; i < 100; i++)
        {
            lock (LockManager.GameServerListLock)
            {
                var server = ServerList.Find(server => server.EntityId == id);
                if (server != null) return server;
            }

            GetServerList(10 * i, 10, false);
        }

        return null;
    }
}