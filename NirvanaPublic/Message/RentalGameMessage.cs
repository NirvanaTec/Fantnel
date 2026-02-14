using NirvanaAPI.Utils.CodeTools;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame.GameCharacters;
using WPFLauncherApi.Protocol;

namespace NirvanaPublic.Message;

public static class RentalGameMessage {
    // 服务器列表[普通信息] - 缓存
    private static Dictionary<string, EntityRentalGameDetails> _serverList = [];

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
            // ServerList 有 就用缓存
            // 分页
            if (_serverList.Count >= count) {
                var list = _serverList.Skip(offset).Take(pageSize).ToArray();
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
                var items = await WPFLauncher.GetRentalGameListAsync();
                AddServerList(items);
                Thread.Sleep(400);
            }
        }
    }

    // 排序 按 PlayerCount 高到低
    public static void SortServerList()
    {
        _serverList = _serverList.OrderByDescending(x => x.Value.PlayerCount).ToDictionary(x => x.Key, x => x.Value);
    }

    private static EntityRentalGameDetails[] GetServerList(KeyValuePair<string, EntityRentalGameDetails>[] serverList)
    {
        return serverList.Select(x => x.Value).ToArray();
    }

    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityRentalGameDetails gameItem)
    {
        if (_serverList.Any(item => item.Value.EntityId == gameItem.EntityId)) {
            return;
        }

        _serverList.Add(gameItem.EntityId, gameItem);
    }

    private static void AddServerList(EntityRentalGame[] gameItem)
    {
        foreach (var item in gameItem) {
            var details = WPFLauncher.GetRentalGameDetailsAsync(item.EntityId).Result;
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
                var games = await WPFLauncher.GetRentalGameRolesListAsync(serverId);
                if (games == null) throw new ErrorCodeException(ErrorCode.NotFound);
                foreach (var game in games)
                    if (game.Name == name)
                        return game;
            } catch (Exception e) {
                Log.Error("获取游戏角色 {0} 失败 {1}", name, e.Message);
            }

            Thread.Sleep(800);
        }

        throw new ErrorCodeException(ErrorCode.NotFoundName);
    }
}