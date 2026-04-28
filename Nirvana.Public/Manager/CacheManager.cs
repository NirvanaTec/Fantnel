using Nirvana.Game.Launcher.Utils;
using Nirvana.Public.Message;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.RentalGame;
using NirvanaAPI.Utils;
using Serilog;

namespace Nirvana.Public.Manager;

public class CacheManager {
    // 缓存图片[待下载]
    private static readonly List<EntityNetGameItem> CacheNet = [];
    private static readonly List<EntityQueryNetSkinItem> CacheSkin = [];
    private static readonly List<EntityRentalGameDetails> CacheRental = [];
    private static readonly Lock CacheImageLock = new();

    // 服务器列表缓存
    private static readonly Lock CacheServerLock = new();

    public static void CacheServer()
    {
        _ = Task.Run(() => {
            lock (CacheServerLock) {
                var threads = new List<Thread> {
                    new(() => {
                        try {
                            if (RentalGameMessage.ServerList.Count < 20) {
                                Log.Information("正在开始缓存 Rental 服务器列表");
                                RentalGameMessage.GetServerList(0, 290).Wait();
                            }
                        } catch (Exception e) {
                            Log.Error("顶缓存 Rental 出错 : {0}", e.Message);
                        }
                    }),
                    new(() => {
                        try {
                            if (ServersGameMessage.ServerList.Count < 20) {
                                Log.Information("正在开始缓存 Net 服务器列表");
                                ServersGameMessage.GetServerListTo(0, 130).Wait();
                            }
                        } catch (Exception e) {
                            Log.Error("顶缓存 Net 出错 : {0}", e.Message);
                        }
                    }),
                    new(() => {
                        try {
                            if (SkinMessage.SkinList.Count < 20) {
                                Log.Information("正在开始缓存 Skin 服务器列表");
                                SkinMessage.GetSkinList(0, 600).Wait();
                            }
                        } catch (Exception e) {
                            Log.Error("顶缓存 Skin 出错 : {0}", e.Message);
                        }
                    })
                };
                foreach (var thread in threads) {
                    thread.Start();
                    Thread.Sleep(500);
                }

                foreach (var thread in threads) {
                    thread.Join();
                }
            }
        });
    }

    // 缓存图片
    public static void GetCacheImageUrl(EntityNetGameItem item)
    {
        lock (CacheImageLock) {
            var filePath = GetCacheImagePath(item.EntityId, "net");
            if (File.Exists(filePath)) {
                item.TitleImageUrl = "/image/net/" + item.EntityId + ".png";
                return;
            }

            if (CacheNet.Any(cache => cache.EntityId == item.EntityId)) {
                return;
            }

            CacheNet.Add(item);
        }
    }

    // 缓存图片
    public static void GetCacheImageUrl(EntityRentalGameDetails item)
    {
        lock (CacheImageLock) {
            var filePath = GetCacheImagePath(item.EntityId, "rental");
            if (File.Exists(filePath)) {
                item.ImageUrl = "/image/rental/" + item.EntityId + ".png";
                return;
            }

            if (CacheRental.Any(cache => cache.EntityId == item.EntityId)) {
                return;
            }

            CacheRental.Add(item);
        }
    }

    public static void GetCacheImageUrl(EntityQueryNetSkinItem item)
    {
        lock (CacheImageLock) {
            var filePath = GetCacheImagePath(item.EntityId, "skin");
            if (File.Exists(filePath)) {
                item.TitleImageUrl = "/image/skin/" + item.EntityId + ".png";
                return;
            }

            if (CacheSkin.Any(cache => cache.EntityId == item.EntityId)) {
                return;
            }

            CacheSkin.Add(item);
        }
    }

    // 清理缓存图片
    private static void ClearCacheImageA(EntityQueryNetGameDetailItem item)
    {
        try {
            lock (CacheImageLock) {
                File.Delete(GetCacheImagePath(item.EntityId, "net"));
                foreach (var server in ServersGameMessage.ServerList.Where(server => server.EntityId == item.EntityId)) {
                    server.TitleImageUrl = item.BriefImageUrls[0];
                    break;
                }
            }
        } catch (Exception e) {
            Log.Error("清理缓存图片 {0} 失败 : {1}", item.EntityId, e.Message);
        }
    }

    // 清理缓存图片
    private static void ClearCacheImageA(EntityRentalGameDetails item)
    {
        try {
            lock (CacheImageLock) {
                File.Delete(GetCacheImagePath(item.EntityId, "rental"));
                foreach (var rental in RentalGameMessage.ServerList.Where(rental => rental.Value.EntityId == item.EntityId)) {
                    rental.Value.ImageUrl = item.ImageUrl;
                    break;
                }
            }
        } catch (Exception e) {
            Log.Error("清理缓存图片 {0} 失败 : {1}", item.EntityId, e.Message);
        }
    }

    // 清理缓存图片
    private static void ClearCacheImageA(EntityQueryNetSkinItem item)
    {
        try {
            lock (CacheImageLock) {
                File.Delete(GetCacheImagePath(item.EntityId, "skin"));
                foreach (var skin in SkinMessage.SkinList.Where(skin => skin.EntityId == item.EntityId)) {
                    skin.TitleImageUrl = item.TitleImageUrl;
                    break;
                }
            }
        } catch (Exception e) {
            Log.Error("清理缓存图片 {0} 失败 : {1}", item.EntityId, e.Message);
        }
    }

    // 清理缓存图片
    public static void ClearCacheImage(EntityQueryNetGameDetailItem item)
    {
        _ = Task.Run(() => ClearCacheImageA(item));
    }

    // 清理缓存图片
    public static void ClearCacheImage(EntityRentalGameDetails item)
    {
        _ = Task.Run(() => ClearCacheImageA(item));
    }

    // 清理缓存图片
    public static void ClearCacheImage(EntityQueryNetSkinItem item)
    {
        _ = Task.Run(() => ClearCacheImageA(item));
    }

    // 获取缓存图片路径
    private static string GetCacheImagePath(string entityId, string name)
    {
        return Path.Combine(PathUtil.CacheImagePath, name, entityId + ".png");
    }

    // 缓存图片下载
    public static void DownloadCacheImage()
    {
        _ = Task.Run(() => {
            lock (CacheImageLock) {
                try {
                    foreach (var item in CacheNet.ToArray()) {
                        CacheNet.Remove(item);
                        _ = DownloadCacheImage(item);
                        // 请求速限制 1 秒 / 12次 ≈ 0.083
                        Thread.Sleep(83);
                    }

                    foreach (var item in CacheRental.ToArray()) {
                        CacheRental.Remove(item);
                        _ = DownloadCacheImage(item);
                        // 请求速限制 1 秒 / 12次 ≈ 0.083
                        Thread.Sleep(83);
                    }

                    foreach (var item in CacheSkin.ToArray()) {
                        CacheSkin.Remove(item);
                        _ = DownloadCacheImage(item);
                        // 请求速限制 1 秒 / 12次 ≈ 0.083
                        Thread.Sleep(83);
                    }
                } catch (Exception e) {
                    Log.Error("缓存图片下载失败 : {0}", e.Message);
                }
            }
        });
    }

    // 缓存图片下载
    private static async Task DownloadCacheImage(EntityNetGameItem item)
    {
        if (string.IsNullOrEmpty(item.TitleImageUrl)) {
            return;
        }

        // 下载图片
        var filePath = GetCacheImagePath(item.EntityId, "net");
        var tmpPath = filePath + ".tmp";
        if (await DownloadUtil.DownloadAsync(item.TitleImageUrl, tmpPath)) {
            File.Move(tmpPath, filePath);
            item.TitleImageUrl = "/image/net/" + item.EntityId + ".png";
        } else {
            Log.Error("缓存 Net 图片 {0} 失败", item.EntityId);
        }
    }

    // 缓存图片下载
    private static async Task DownloadCacheImage(EntityRentalGameDetails item)
    {
        if (string.IsNullOrEmpty(item.ImageUrl)) {
            return;
        }

        // 下载图片
        var filePath = GetCacheImagePath(item.EntityId, "rental");
        var tmpPath = filePath + ".tmp";
        if (await DownloadUtil.DownloadAsync(item.ImageUrl, tmpPath)) {
            File.Move(tmpPath, filePath);
            item.ImageUrl = "/image/rental/" + item.EntityId + ".png";
        } else {
            Log.Error("缓存 Rental 图片 {0} 失败", item.EntityId);
        }
    }

    // 缓存图片下载
    private static async Task DownloadCacheImage(EntityQueryNetSkinItem item)
    {
        if (string.IsNullOrEmpty(item.TitleImageUrl)) {
            return;
        }

        // 下载图片
        var filePath = GetCacheImagePath(item.EntityId, "skin");
        var tmpPath = filePath + ".tmp";
        if (await DownloadUtil.DownloadAsync(item.TitleImageUrl, tmpPath)) {
            File.Move(tmpPath, filePath);
            item.TitleImageUrl = "/image/skin/" + item.EntityId + ".png";
        } else {
            Log.Error("缓存 Skin 图片 {0} 失败", item.EntityId);
        }
    }
}