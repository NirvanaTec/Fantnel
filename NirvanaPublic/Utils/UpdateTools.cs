using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using Serilog;
using WPFLauncherApi.Http;

namespace NirvanaPublic.Utils;

public static class UpdateTools
{
    // 检查更新
    public static async Task CheckUpdate(string[] args)
    {
        var update = 0; // 0:正常检查 1:不检查 2:已被检查
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            // // win 系统不检查更新
            // // 因为 win 运行后文件被占用时不能覆盖
            // update = 1;
            if (args.Any(arg => arg == "--update_false"))
                update = 2;

        switch (update)
        {
            // 不检查 - 提醒
            // case 1:
            // {
            //     if (PublicProgram.Release)
            //     {
            //         for (var i = 0; i < 4; i++) Log.Warning("当前版本已取消自动更新，建议前往官网重新下载！");
            //
            //         Thread.Sleep(3000);
            //     }
            //
            //     break;
            // }
            // 正常检查
            case 0:
                if (PublicProgram.Release)
                    await CheckUpdate(PublicProgram.Mode + "." + PublicProgram.Arch, "Fantnel", true);
                break;
        }

        await CheckUpdate("static", "Resource");
        await CheckUpdate("static." + PublicProgram.Mode, "Resource");
        await CheckUpdate("static." + PublicProgram.Mode + "." + PublicProgram.Arch, "Resource", false,false);
    }

    /**
     * 检查更新
     * @param name 名称
     * @param safe 是否安全模式
     */
    public static async Task<int> CheckUpdate(string mode, string name = "", bool safe = false, bool failureLog = true)
    {
        var jsonObj =
            await X19Extensions.Nirvana.Api<JsonObject>(
                $"/api/fantnel/update/get?mode={mode}");
        if (jsonObj == null)
        {
            if (!failureLog) return -1;
            Log.Error("{name}: {mode}", name, mode);
            Log.Error("检查更新失败, 建议更新至最新版本!");
            return -1;
        }

        var data = jsonObj["data"];
        if (data == null)
        {
            if (!failureLog) return -1;
            Log.Error("{name}: {mode}", name, mode);
            Log.Error("检查更新失败, 建议更新至最新版本!");
            return -1;
        }

        var array = data.AsArray();
        await ThreadUpdateTools.CheckUpdate(array, name, safe);
        return array.Count;
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
        var progress = new SyncProgressBarUtil.ProgressBar();
        // 下载插件 进度条 回调
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update =>
            progress.Update(update.Percent, update.Message));

        await DownloadUtil.DownloadAsync(url, path, dp =>
        {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport
            {
                Percent = dp,
                Message = $"Downloading {name}"
            });
        });
    }
}