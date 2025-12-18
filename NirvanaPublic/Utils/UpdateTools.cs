using System.Text.Json;
using System.Text.Json.Nodes;
using Codexus.Cipher.Utils.Http;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using Serilog;

namespace NirvanaPublic.Utils;

public static class UpdateTools
{
    // 检查更新的模式
    public const string Mode = "win64"; // win64 | linux64 | mac64

    // 检查更新
    public static async Task CheckUpdate()
    {
        await CheckUpdate(Mode);
        await CheckUpdate("static");
    }

    /**
     * 检查更新
     */
    private static async Task CheckUpdate(string mode)
    {
        var http = new HttpWrapper("http://110.42.70.32:13423");
        var response = await http.GetAsync($"/api/fantnel/update/get?mode={mode}");
        var json = await response.Content.ReadAsStringAsync();
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        if(jsonObj == null)
        {
            Log.Error("检查更新失败, 建议更新至最新版本!");
            return;
        }
        var data = jsonObj["data"];
        if(data == null)
        {
            Log.Error("检查更新失败, 建议更新至最新版本!");
            return;
        }
        await CheckUpdate(data.AsArray());
    }

    /**
     * 检查更新
     *
      "path": "Development.json",
      "size": 127,
      "url": "http://110.42.70.32:23148/fantnel/update/win64/Development.json",
      "sha256": "73f95f9e0ceb205fc1c4dc50c0769729d7087868c2aef1d504cb38c771ec"
     */
    private static async Task CheckUpdate(JsonArray jsonArray)
    {
        foreach (var item in jsonArray)
        {
            
            if(item == null)
            {
                continue;
            }
            
            // 没有地址 / 没有路径 跳过
            var url = item["url"]?.GetValue<string>();
            var path = item["path"]?.GetValue<string>();
            if(string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path))
            {
                continue;
            }
            
            // 修复 linux 路径
            path = path.Replace('\\', Path.DirectorySeparatorChar);

            var start = false; // 是否需要更新
            var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            
            // 文件 是否存在 检测
            if (!File.Exists(resourcesPath))
            {
                start = true;
            }

            if (!start)
            {
                // 文件 大小 检测
                var size = item["size"];
                if (size != null)
                {
                    var fileSize = new FileInfo(resourcesPath);
                    if (fileSize.Length != size.GetValue<long>())
                    {
                        start = true;
                    }
                }
            }

            if (!start)
            {
                // 文件 sha256 检测
                var sha256 = item["sha256"];
                if (sha256 != null)
                {
                    // 计算文件 sha256
                    var fileSha256 = Tools.ComputeSha256(resourcesPath);
                    if (fileSha256 != sha256.GetValue<string>())
                    {
                        start = true;
                    }
                }
            }
            
            if(!start)
            {
                continue;
            }
            await Update(url, resourcesPath, path);
        }
    }

    
    /**
     * 更新文件
     * @param url 下载地址
     * @param path 保存路径
     * @param name 下载名称
     */
    public static async Task Update(string url, string path, string name)
    {
        // 下载插件 进度条 初始化
        var progress = new SyncProgressBarUtil.ProgressBar(100);
        // 下载插件 进度条 回调
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update =>
            progress.Update(update.Percent, update.Message));
        await DownloadUtil.DownloadAsync(url, path, (Action<uint>)(dp =>
        {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport
            {
                Percent = (int)dp,
                Message = $"Downloading {name}"
            });
        }));
    }

}