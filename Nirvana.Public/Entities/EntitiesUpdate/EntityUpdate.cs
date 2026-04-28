using System.Text.Json.Nodes;
using Nirvana.Game.Launcher.Utils.Progress;
using Nirvana.WPFLauncher.Http;
using NirvanaAPI.Utils;
using Serilog;
using FileUtil = Nirvana.Public.Utils.Update.FileUtil;

namespace Nirvana.Public.Entities.EntitiesUpdate;

public class EntityUpdate {
    private static readonly Lock Lock = new();
    public string Command = ""; // 附加 脚本 命令 [更新\n + "Command" + \n启动]

    public required string Mode; // 更新模式名 - win.x64
    public string Name = "Resource"; // 显示名称
    public bool SafeMode = false; // 使用 脚本 更新

    private async Task<JsonArray?> Initialize()
    {
        var jsonObj = await X19Extensions.Nirvana.Api<JsonObject>($"/api/fantnel/update/get?mode={Mode}");
        if (jsonObj == null) {
            WriteLine("获取更新信息失败，请检查网络连接。");
            return null;
        }

        var data = jsonObj["data"];
        if (data == null) {
            WriteLine("获取更新信息出错，请检查网络连接。");
            return null;
        }

        return data.AsArray();
    }

    /**
     * 检查单个文件更新 [仅用于单文件更新]
     * @param filePath 完整文件路径
     * @return 0无需 1成功
     */
    public async Task<int> CheckUpdateSingle(string filePath, Action<double>? downloadProgress = null)
    {
        var jsonArray = await Initialize();
        if (jsonArray == null) {
            return -1;
        }

        if (jsonArray.Count > 1) {
            return WriteLine("获取更新信息异常，请检查网络连接。");
        }

        var success = await CheckUpdateSingle(jsonArray[0], filePath, downloadProgress);
        return await SafeRestartUpdate(success, filePath);
    }

    /**
     * 检查所有文件更新 [完全保证更新成功]
     * @param basePath 基础文件路径 [UpdaterBasePath + "basePath" + FilePath]
     * @return 影响文件数量 [仅包含: 0无需 1成功]
     */
    public async Task<int> CheckUpdateSafe(params string[] basePathList)
    {
        while (true) {
            var count = await CheckUpdate(basePathList);
            if (count == 0) {
                return 0;
            }

            Thread.Sleep(500);
        }
    }

    /**
     * 检查所有文件更新
     * @param basePath 基础文件路径 [UpdaterBasePath + "basePath" + FilePath]
     * @return 影响文件数量 [仅包含: 0无需 1成功]
     */
    private async Task<int> CheckUpdate(params string[] basePathList)
    {
        // 下载插件 进度条 初始化
        var progressBar = new SyncProgressBarUtil.ProgressBar();
        // 下载插件 进度条 回调
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update => progressBar.Update(update.Percent, update.Message));
        return await CheckUpdate(dp => {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = dp,
                Message = $"Downloading {Name}"
            });
        }, basePathList);
    }

    /**
     * 检查所有文件更新
     * @param basePath 基础文件路径 [UpdaterBasePath + "basePath" + FilePath]
     * @return 影响文件数量 [仅包含: 0无需 1成功]
     */
    private async Task<int> CheckUpdate(Action<double>? downloadProgress = null, params string[] basePathList)
    {
        var jsonArray = await Initialize();
        if (jsonArray == null) {
            return -1;
        }

        if (SafeMode && Directory.Exists(PathUtil.UpdaterPath)) {
            Directory.Delete(PathUtil.UpdaterPath, true);
        }

        var count = 0;
        var progress = 0;
        var threads = new List<Thread>();
        for (var i = 0; i < jsonArray.Count; i++) {
            var jsonNode = jsonArray[i];
            var entityUpdate = new EntityUpdateFile(jsonNode) {
                Index = i
            };
            var filePath = entityUpdate.GetPath(false, basePathList);
            if (string.IsNullOrEmpty(filePath)) {
                WriteLine($"无法获取 [{entityUpdate.Index}] 的保存路径！");
                return -2;
            }

            var safeSavePath = filePath;
            if (SafeMode) {
                safeSavePath = entityUpdate.GetPath(true, basePathList);
                if (string.IsNullOrEmpty(safeSavePath)) {
                    WriteLine($"无法获取 [{entityUpdate.Index}] 的保存路径！");
                    return -3;
                }
            }

            var thread = new Thread(() => {
                // Log.Warning("{0}: Start...", entityUpdate.Index);
                var success = CheckUpdateSingle(entityUpdate, filePath, safeSavePath).Result;
                lock (Lock) {
                    if (success == 1) {
                        count++;
                    }

                    // Log.Warning("{0}: {1}", entityUpdate.Index, success);
                    progress++;
                    downloadProgress?.Invoke(100.0 / jsonArray.Count * progress);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads) {
            thread.Join();
        }

        if (count > 0) {
            await SafeRestartUpdate(1);
        }

        return count;
    }

    /**
     * 检查单个文件更新
     * @param filePath 完整文件路径
     * @return 0无需 1成功
     */
    private async Task<int> CheckUpdateSingle(JsonNode? jsonNode, string filePath, Action<double>? downloadProgress = null)
    {
        var entityUpdate = new EntityUpdateFile(jsonNode) {
            Index = 0
        };
        var safeSavePath = filePath;
        if (SafeMode) {
            if (Directory.Exists(PathUtil.UpdaterPath)) {
                Directory.Delete(PathUtil.UpdaterPath, true);
            }

            safeSavePath = Path.Combine(PathUtil.UpdaterPath, Path.GetFileName(filePath));
        }

        return await CheckUpdateSingle(entityUpdate, filePath, safeSavePath, downloadProgress);
    }

    /**
     * 检查单个文件更新
     * @param filePath 完整文件路径
     * @return 0无需 1成功
     */
    private async Task<int> CheckUpdateSingle(EntityUpdateFile entityUpdate, string filePath, string safeSavePath, Action<double>? downloadProgress = null)
    {
        var success = await entityUpdate.CheckUpdate(filePath, safeSavePath, downloadProgress);
        return success switch {
            2 => WriteLine("更新时出现致命错误，请前往官网重新下载。"),
            3 => WriteLine($"无法获取 [{entityUpdate.Index}] 的下载地址！"),
            _ => success
        };
    }

    /**
     * 安全重启更新
     * @param success 更新结果
     * @param filePath 完整文件路径
     */
    private async Task<int> SafeRestartUpdate(int success, string? filePath = null)
    {
        if (success == 1 && SafeMode) {
            await FileUtil.SafeRestartUpdate(Command, filePath);
        }

        return success;
    }

    private int WriteLine(string message)
    {
        Log.Warning("{0}: {1}", Name, Mode);
        Log.Warning("{0}", message);
        Environment.Exit(1);
        return -1;
    }
}