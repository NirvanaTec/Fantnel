using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.RentalGame;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.RentalGame.GameCharacters;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Public.Message;

public static class RentalGameMessage {
    // 服务器列表[普通信息] - 缓存
    public static Dictionary<string, EntityRentalGameDetails> ServerList = [];

    /**
     * 获取服务器列表[普通信息]
     * @param offset 偏移量
     * @param pageSize 每页数量
     * @return 服务器列表[普通信息]
     */
    public static async Task<EntityRentalGameDetails[]> GetServerList(int offset = 0, int pageSize = 10)
    {
        var index = -pageSize; // 循环次数
        var count = offset + pageSize;

        while (true) {
            // 缓存图片下载
            CacheManager.DownloadCacheImage();

            // ServerList 有 就用缓存
            // 分页
            if (ServerList.Count >= count) {
                var list = ServerList.Skip(offset).Take(pageSize).ToArray();
                return GetServerList(list);
            }

            if (++index > 0) {
                // 最后一页, 减少数量，避免丢失数据
                count--;
                pageSize--;
                if (pageSize <= 0) {
                    return [];
                }
            } else {
                var items = await NPFLauncher.GetRentalGameListAsync();
                AddServerList(items);
                Thread.Sleep(540);
            }
        }
    }

    // 排序 按 PlayerCount 高到低
    public static void SortServerList()
    {
        ServerList = ServerList.OrderByDescending(x => x.Value.PlayerCount).ToDictionary(x => x.Key, x => x.Value);
    }

    private static EntityRentalGameDetails[] GetServerList(KeyValuePair<string, EntityRentalGameDetails>[] serverList)
    {
        return serverList.Select(x => x.Value).ToArray();
    }

    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityRentalGameDetails gameItem)
    {
        if (ServerList.Any(item => item.Value.EntityId == gameItem.EntityId)) {
            return;
        }

        CacheManager.GetCacheImageUrl(gameItem);
        ServerList.Add(gameItem.EntityId, gameItem);
    }

    private static void AddServerList(EntityRentalGame[] gameItem)
    {
        foreach (var item in gameItem) {
            var details = NPFLauncher.GetRentalGameDetailsAsync(item.EntityId).Result;
            AddServerList(details);
        }
    }

    /**
     * 获取服务器上的指定游戏角色
     * @param serverId 服务器ID
     * @param name 游戏角色名称
     * @return 服务器上的指定游戏角色
     */
    public static async Task<EntityRentalGamePlayerList> GetUserName(string serverId, string name)
    {
        for (var i = 0; i < 3; i++) {
            try {
                var games = await NPFLauncher.GetRentalGameRolesListAsync(serverId);
                if (games == null) {
                    throw new ErrorCodeException(ErrorCode.NotFound);
                }

                foreach (var game in games) {
                    if (game.Name == name) {
                        return game;
                    }
                }
            } catch (Exception e) {
                Log.Error("获取游戏角色 {0} 失败 {1}", name, e.Message);
            }

            Thread.Sleep(800);
        }

        throw new ErrorCodeException(ErrorCode.NotFoundName);
    }
}