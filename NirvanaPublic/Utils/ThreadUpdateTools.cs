using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using NirvanaAPI.Utils;
using NirvanaPublic.Entities;
using Serilog;
using Tools = NirvanaAPI.Utils.Tools;

namespace NirvanaPublic.Utils;

public static class ThreadUpdateTools {
    /**
     * 检查更新
     * path: Fantnel1.dll,
     * size: 127,
     * url: http://npyyds.to/Fantnel1.dll,
     * sha256: 73f95f9e0ceb205fc1c4dc50c0769729d7087868c2aef1d504cb38c771ec
     */
    public static async Task CheckUpdate(JsonArray jsonArray, string name, bool safeMode = false)
    {
        var downloadSize = 0;
        List<IntPtrReference> progress = [];

        foreach (var item in jsonArray) {
            // 下载进度
            var newProgress = new IntPtrReference();
            progress.Add(newProgress);

            if (item == null) continue;

            var url = item["url"]?.GetValue<string>();
            var pathValue = item["path"]?.GetValue<string>();
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(pathValue)) continue;

            // 修复路径
            pathValue = pathValue.Replace('\\', Path.DirectorySeparatorChar);

            var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathValue);
            var resourcesPath1 = resourcesPath;
            if (safeMode) resourcesPath1 = Path.Combine(PathUtil.UpdaterPath, pathValue);

            // 硬盘访问速限制 1 秒 / 32次 ≈ 0.015
            Thread.Sleep(15);

            // 检查是否需要更新
            if (!NeedsUpdate(item, resourcesPath)) continue;

            // 请求速限制 1 秒 / 12次 ≈ 0.083
            // 83 - 15 = 68ms
            Thread.Sleep(68);
            newProgress.Value = 0;
            if (pathValue.EndsWith(".dll")) downloadSize++;
            DownloadWithRetryAsync(url, resourcesPath1, newProgress, name, progress, jsonArray.Count);
        }

        if (safeMode && downloadSize > 0) {
            Log.Information("正在更新核心资源，这会自动重启[1次]，请稍后...");
            var scriptPath = PathUtil.ScriptPath;
            await Tools.SaveShellScript(scriptPath, GenerateUpdateScript());
            Process.Start(new ProcessStartInfo {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "/C \"" + scriptPath + "\""
                    : scriptPath,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            Environment.Exit(0);
        }
    }

    private static string GenerateUpdateScript()
    {
        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Tools.GetProcessLocation()
            : Tools.GetProcessArguments();
        var updateScript = GenerateUpdateScript(PathUtil.UpdaterPath, AppDomain.CurrentDomain.BaseDirectory, exeName);
        Log.Information("更新脚本: {UpdateScript}", updateScript);
        return updateScript;
    }

    private static string GenerateUpdateScript(string tempDir, string targetDir, string exeName)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"timeout /t 1 /nobreak\r\nxcopy /e /y /i \"{tempDir}\\*\" \"{targetDir}\"\r\nstart \"\" \"{exeName}\""
            : $"sleep 1\ncp -r \"{tempDir}/.\" \"{targetDir}\"\ndotnet \"{exeName}\" & rm -rf \"{tempDir}\"";
    }

    private static void DownloadWithRetryAsync(string url, string filePath, IntPtrReference progressRef,
        string name, List<IntPtrReference> allProgress, int totalCount)
    {
        DownloadUtil.DownloadAsync(url, filePath, progressValue => { progressRef.Value = progressValue; }).Wait();
        UpdateProgress(allProgress, totalCount, name);
    }

    private static void UpdateProgress(List<IntPtrReference> progress, int totalCount, string name)
    {
        // 当前下载进度 总和
        var sumPercent = progress.Aggregate(0.0, (current, reference) => current + reference.Value);
        // 当前下载进度 平均值
        var totalProgress = sumPercent / totalCount;
        var completedCount = progress.Count(p => p.Value == 100);

        var progressBar = new SyncProgressBarUtil.ProgressBar();
        progressBar.Update(totalProgress, $"Downloading {completedCount} / {totalCount} {name}");
    }

    private static bool NeedsUpdate(JsonNode item, string filePath)
    {
        // 文件是否存在
        if (!File.Exists(filePath)) return true;

        // 检查文件大小
        var size = item["size"];
        if (size != null) {
            var expectedSize = size.GetValue<long>();
            var actualSize = new FileInfo(filePath).Length;
            if (actualSize != expectedSize) return true;
        }

        // 检查SHA256
        var sha256 = item["sha256"];
        if (sha256 == null) return false;
        var expectedHash = sha256.GetValue<string>();
        var actualHash = Tools.ComputeSha256(filePath);
        return !string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}