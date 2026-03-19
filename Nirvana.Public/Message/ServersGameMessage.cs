using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Public.Message;

public static class ServersGameMessage {
    // 服务器列表[普通信息] - 缓存
    public static readonly List<EntityNetGameItem> ServerList = [];

    /**
     * 获取服务器列表[普通信息]
     * @param offset 偏移量
     * @param pageSize 每页数量
     * @param safeImage 是否安全获取图片
     * @return 服务器列表[普通信息]
     */
    private static async Task<EntityNetGameItem[]> GetServerList(int offset = 0, int pageSize = 10,
        bool safeImage = true)
    {
        var index = -pageSize;
        var count = pageSize + offset;

        while (true) {
            // ServerList 有 就用缓存
            // 分页
            if (ServerList.Count >= count) {
                var list = ServerList.Skip(offset).Take(pageSize).ToArray();

                // 缓存图片下载
                CacheManager.DownloadCacheImage();

                // 无须修复图片
                if (!safeImage) {
                    return list;
                }

                // 修复没有图片的游戏项
                foreach (var item in list) {
                    // 没有图片
                    if (item.TitleImageSafe()) {
                        continue;
                    }

                    // 从 详情页 获取图片
                    await GetFirstImageAndVerByCache(item);
                }

                return list;
            }

            if (++index > 0) {
                // 最后一页, 减少数量，避免丢失数据
                count--;
                pageSize--;
                if (pageSize <= 0) {
                    return [];
                }
            } else {
                var items = await NPFLauncher.GetAvailableNetGamesAsync(ServerList.Count);
                if (items.Length == 0) {
                    index = 1;
                }

                AddServerList(items);
                Thread.Sleep(500);
            }
        }
    }

    public static async Task<EntityNetGameItem[]> GetServerListTo(int offset = 0, int pageSize = 10,
        bool safeImage = true, string version = "")
    {
        // 没有版本号, 直接获取
        if (string.IsNullOrEmpty(version)) {
            return await GetServerList(offset, pageSize, safeImage);
        }

        return await GetServerListTo(offset, pageSize, safeImage, item => item.Version == version);
    }

    private static async Task<EntityNetGameItem[]> GetServerListTo(int offset, int pageSize,
        bool safeImage, Func<EntityNetGameItem, bool> filter)
    {
        var index = -pageSize;
        var count = pageSize + offset;
        var pageSize1 = ServerList.Count;

        while (true) {
            var list = await GetServerList(0, pageSize1, safeImage);
            var list1 = list.Where(filter.Invoke).ToArray();
            if (list1.Length >= count) {
                return list1.Skip(offset).Take(pageSize).ToArray();
            }

            if (++index > 0) {
                // 最后一页, 减少数量，避免丢失数据
                count--;
                pageSize--;
                if (pageSize <= 0) {
                    return [];
                }
            } else {
                var items = await NPFLauncher.GetAvailableNetGamesAsync(ServerList.Count);
                if (items.Length == 0) {
                    index = 1;
                } else {
                    pageSize1 += 20;
                }

                AddServerList(items);
                Thread.Sleep(500);
            }
        }
    }

    // 获取 主页图片 / 版本
    private static async Task GetFirstImageAndVer(EntityNetGameItem item)
    {
        var details = await NPFLauncher.GetNetGameDetailByIdAsync(item.EntityId);
        // if (details != null && details.BriefImageUrls.Length > 0)
        item.Version = details is { McVersionList.Length: > 0 } ? details.McVersionList[0].Name : "";
        item.TitleImageUrl = details is { BriefImageUrls.Length: > 0 } ? details.BriefImageUrls[0] : "";
    }

    // 获取 主页图片[缓存保存] / 版本
    private static async Task GetFirstImageAndVerByCache(EntityNetGameItem item)
    {
        await GetFirstImageAndVer(item);
        CacheManager.GetCacheImageUrl(item);
    }

    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityNetGameItem gameItem)
    {
        foreach (var item in ServerList.Where(item => item.EntityId == gameItem.EntityId)) {
            if (item.TitleImageUrl == "" && gameItem.TitleImageUrl != "") {
                item.TitleImageUrl = gameItem.TitleImageUrl;
                CacheManager.GetCacheImageUrl(item);
            }

            return;
        }

        ServerList.Add(gameItem);
    }

    // 服务器列表[普通信息] - 添加
    private static void AddServerList(EntityNetGameItem[] gameItems)
    {
        foreach (var item in gameItems) {
            AddServerList(item);
        }
    }

    /**
     * 获取服务器上的指定游戏角色
     * @param serverId 服务器ID
     * @param name 游戏角色名称
     * @return 服务器上的指定游戏角色
     */
    public static async Task<EntityGameCharacter> GetUserName(string serverId, string name)
    {
        for (var i = 0; i < 3; i++) {
            try {
                var games = await NPFLauncher.GetNetGameCharactersAsync(serverId);
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