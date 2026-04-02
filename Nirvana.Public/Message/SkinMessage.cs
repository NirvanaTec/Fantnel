using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using Nirvana.WPFLauncher.Protocol;

namespace Nirvana.Public.Message;

public static class SkinMessage {
    // 皮肤列表 - 缓存
    public static readonly List<EntityQueryNetSkinItem> SkinList = [];

    public static async Task<EntityQueryNetSkinItem[]> GetSkinList(int offset = 0, int pageSize = 10,
        bool safeImage = true)
    {
        var index = -pageSize; // 循环次数
        var count = offset + pageSize;

        while (true) {
            // 缓存图片下载
            CacheManager.DownloadCacheImage();

            // ServerList 有 就用缓存
            // 分页
            if (SkinList.Count >= count) {
                var list = SkinList.Skip(offset).Take(pageSize).ToArray();
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
                    await GetFirstImageByCache(item);
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
                var items = await NPFLauncher.GetFreeSkinListAsync(SkinList.Count);
                AddSkinList(items);
                Thread.Sleep(500);
            }
        }
    }

    private static void AddSkinList(EntityQueryNetSkinItem[] skinItems)
    {
        foreach (var item in skinItems) {
            AddSkinList(item);
        }
    }

    // 皮肤列表 - 添加
    private static void AddSkinList(EntityQueryNetSkinItem skinItem)
    {
        foreach (var item in SkinList.Where(item => item.EntityId == skinItem.EntityId)) {
            if (item.TitleImageUrl == "" && skinItem.TitleImageUrl != "") {
                item.TitleImageUrl = skinItem.TitleImageUrl;
                CacheManager.GetCacheImageUrl(item);
            }

            return;
        }

        SkinList.Add(skinItem);
    }

    /**
     * 获取皮肤的第一张图片
     * 来自详情页
     * @param entityId 皮肤ID
     * @return 图片URL
     */
    private static async Task GetFirstImage(EntityQueryNetSkinItem item)
    {
        var details = await NPFLauncher.GetSkinDetailsAsync(item.EntityId);
        item.TitleImageUrl = details.TitleImageUrl;
    }

    // 获取 主页图片[缓存保存] / 版本
    private static async Task GetFirstImageByCache(EntityQueryNetSkinItem item)
    {
        await GetFirstImage(item);
        CacheManager.GetCacheImageUrl(item);
    }

    public static async Task<EntityQueryNetSkinItem[]> GetSkinListByName(string name, int offset = 0, int pageSize = 10)
    {
        var result = NPFLauncher.GetFreeSkinByNameAsync(name, offset, pageSize).Result;

        var items = new List<EntityQueryNetSkinItem>();
        foreach (var item in result) {
            await GetFirstImageByCache(item);
            if (string.IsNullOrEmpty(item.TitleImageUrl)) {
                continue;
            }

            items.Add(item);
        }

        return items.ToArray();
    }
}