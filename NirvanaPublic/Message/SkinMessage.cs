using System.Text.Json;
using Codexus.Cipher.Entities;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Skin;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;

namespace NirvanaPublic.Message;

public static class SkinMessage
{
    // 皮肤列表 - 缓存
    private static readonly List<EntityQueryNetSkinItem> SkinList = [];

    public static EntityQueryNetSkinItem[] GetSkinList(int offset = 0, int pageSize = 10,
        bool safeImage = true, bool refresh = true)
    {
        // SkinList 有 就用缓存
        lock (LockManager.GameServerListLock)
        {
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
                        item.TitleImageUrl = GetFirstImage(item);

                return list;
            }
        }

        var result = GetFreeSkinListAsync(offset, pageSize).Result;

        if (result?.Data == null)
        {
            var error = new Code.ErrorCodeException(Code.ErrorCode.NotFound);
            if (result != null && !string.IsNullOrEmpty(result.Message)) error.Entity.Msg = result.Message;
            throw error;
        }

        // 成功检测
        Tools.EntitySafe(result);

        // safeImage: 没有图片的游戏项 就从 详情页 获取图片
        var items = result.Data;
        foreach (var item in items)
        {
            if (safeImage) item.TitleImageUrl = GetFirstImage(item);
            AddSkinList(item);
        }

        return result.Data;
    }

    private static async Task<Entity1<EntityQueryNetSkinItem[]?>?> GetFreeSkinListAsync(
        int offset,
        int length = 20)
    {
        return await X19Extensions.Api<Entity1<EntityQueryNetSkinItem[]?>>("/item/query/available",
            JsonSerializer.Serialize(new EntityFreeSkinListRequest
            {
                IsHas = true,
                ItemType = 2,
                Length = length,
                MasterTypeId = 10,
                Offset = offset,
                PriceType = 3,
                SecondaryTypeId = 31
            }));
    }

    public static EntitySkin GetSkinDetails(string entityId)
    {
        var skinItem = new EntitySkin
        {
            EntityId = entityId,
            BriefSummary = "",
            Name = "",
            TitleImageUrl = "",
            LikeNum = 0
        };
        var skinList = new Entities<EntitySkin>
        {
            Data = [skinItem]
        };
        return InitProgram.GetServices().Wpf.GetSkinDetails(InfoManager.GetGameAccount().GetUserId(),
            InfoManager.GetGameAccount().GetToken(), skinList).Data[0];
    }

    private static string GetFirstImage(EntityQueryNetSkinItem item)
    {
        return GetSkinDetails(item).TitleImageUrl;
    }

    // 皮肤列表 - 添加
    private static void AddSkinList(EntityQueryNetSkinItem skinItem)
    {
        lock (LockManager.GameServerListLock)
        {
            foreach (var item in SkinList.Where(item => item.EntityId == skinItem.EntityId))
            {
                if (item.TitleImageUrl == "" && skinItem.TitleImageUrl != "")
                    item.TitleImageUrl = skinItem.TitleImageUrl;
                return;
            }

            SkinList.Add(skinItem);
        }
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
            lock (LockManager.GameServerListLock)
            {
                var server = SkinList.Find(server => server.EntityId == id);
                if (server != null) return server;
            }

            GetSkinList(10 * i, 10, false);
        }

        return null;
    }

    private static EntitySkin GetSkinDetails(EntityQueryNetSkinItem item)
    {
        return GetSkinDetails(item.EntityId);
    }

    public static void SetSkin(string id)
    {
        var response = InitProgram.GetServices().Wpf.SetSkin(InfoManager.GetGameAccount().GetUserId(),
            InfoManager.GetGameAccount().GetToken(), id);
        if (response.Code == 0) return;
        var exception = new Code.ErrorCodeException(Code.ErrorCode.Failure)
        {
            Entity =
            {
                Msg = response.Message
            }
        };
        throw exception;
    }

    public static EntityQueryNetSkinItem[] GetSkinListByName(string name, int offset = 0, int pageSize = 10)
    {
        var result = QueryFreeSkinByNameAsync(name, offset, pageSize).Result;
        if (result?.Data == null)
        {
            var error = new Code.ErrorCodeException(Code.ErrorCode.NotFound);
            if (result != null && !string.IsNullOrEmpty(result.Message)) error.Entity.Msg = result.Message;
            throw error;
        }

        // 成功检测
        Tools.EntitySafe(result);
        var items = new List<EntityQueryNetSkinItem>();
        foreach (var item in result.Data)
        {
            item.TitleImageUrl = GetFirstImage(item);
            if (item.TitleImageUrl != "") items.Add(item);
        }

        return items.ToArray();
    }

    private static async Task<Entity1<EntityQueryNetSkinItem[]?>?> QueryFreeSkinByNameAsync(string name, int offset = 0,
        int pageSize = 10)
    {
        return await X19Extensions.Api<Entity1<EntityQueryNetSkinItem[]?>>("/item/query/search-by-keyword",
            JsonSerializer.Serialize(new EntityQuerySkinByNameRequest
            {
                IsHas = true,
                IsSync = 0,
                ItemType = 2,
                Keyword = name,
                Length = pageSize,
                MasterTypeId = 10,
                Offset = offset,
                PriceType = 3,
                SecondaryTypeId = "31",
                SortType = 1,
                Year = 0
            }));
    }
}