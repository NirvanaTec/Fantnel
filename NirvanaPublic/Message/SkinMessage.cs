using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using WPFLauncherApi.Protocol;

namespace NirvanaPublic.Message;

public static class SkinMessage
{
    // 皮肤列表 - 缓存
    private static readonly List<EntityQueryNetSkinItem> SkinList = [];

    public static EntityQueryNetSkinItem[] GetSkinList(int offset = 0, int pageSize = 10,
        bool safeImage = true, bool refresh = true)
    {
        // SkinList 有 就用缓存
        // 分页 异常顺序 检测
        var size = offset + (pageSize - 10);
        if (SkinList.Count < size && refresh)
            // safeImage 加快速度 | refresh 防止递归
            GetSkinList(SkinList.Count, size, false, false);
        // 分页
        size = (offset == 0 ? 1 : offset) * pageSize;
        if (SkinList.Count >= size)
        {
            var list = SkinList.Skip(size - pageSize).Take(pageSize).ToArray();
            if (!safeImage) return list;
            // 修复没有图片的游戏项
            foreach (var item in list)
                // 没有图片
                if (!item.TitleImageSafe())
                    // 从 详情页 获取图片
                    item.TitleImageUrl = WPFLauncher.GetFirstImage(item.EntityId).Result;

            return list;
        }

        var items = WPFLauncher.GetFreeSkinListAsync(offset, pageSize).Result;

        // safeImage: 没有图片的游戏项 就从 详情页 获取图片
        foreach (var item in items)
        {
            if (safeImage) item.TitleImageUrl = WPFLauncher.GetFirstImage(item.EntityId).Result;
            AddSkinList(item);
        }

        return items.ToArray();
    }

    // 皮肤列表 - 添加
    private static void AddSkinList(EntityQueryNetSkinItem skinItem)
    {
        foreach (var item in SkinList.Where(item => item.EntityId == skinItem.EntityId))
        {
            if (item.TitleImageUrl == "" && skinItem.TitleImageUrl != "")
                item.TitleImageUrl = skinItem.TitleImageUrl;
            return;
        }

        SkinList.Add(skinItem);
    }

    /**
     * 皮肤列表[普通信息] - 获取
     * @param id 皮肤ID
     * @return 皮肤信息
     */
    public static EntityQueryNetSkinItem? GetSkinId(string id)
    {
        for (var i = 0; i < 100; i++)
        {
            var server = SkinList.Find(server => server.EntityId == id);
            if (server != null) return server;
            GetSkinList(10 * i, 10, false);
        }

        return null;
    }

    public static async Task<EntityQueryNetSkinItem[]> GetSkinListByName(string name, int offset = 0, int pageSize = 10)
    {
        var result = WPFLauncher.GetFreeSkinByNameAsync(name, offset, pageSize).Result;

        var items = new List<EntityQueryNetSkinItem>();
        foreach (var item in result)
        {
            item.TitleImageUrl = await WPFLauncher.GetFirstImage(item.EntityId);
            if (item.TitleImageUrl != "") items.Add(item);
        }

        return items.ToArray();
    }
}