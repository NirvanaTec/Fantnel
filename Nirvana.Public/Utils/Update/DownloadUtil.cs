using Downloader;
using Nirvana.Game.Launcher.Utils.Progress;
using Serilog;

namespace Nirvana.Public.Utils.Update;

public static class DownloadUtil {
    /**
     * 异步下载文件
     */
    public static async Task<bool> DownloadAsync(string url, string destinationPath,
        Action<double>? downloadProgress = null)
    {
        try {
            var downloadOpt = new DownloadConfiguration {
                // ChunkCount = 1, // 设置并发块数
                MaxTryAgainOnFailure = 4, // 下载失败后重试次数
                // ParallelDownload = true, // 启用并行下载 [ChunkCount]
                EnableAutoResumeDownload = true // 启用自动续传功能
            };

            await using var downloader = new DownloadService(downloadOpt);

            var lastReportTime = DateTime.MinValue;
            var throttlePeriod = TimeSpan.FromMilliseconds(200); // 设置更新间隔为100毫秒

            // 注册进度更新事件
            downloader.DownloadProgressChanged += (_, e) => {
                var percentage = e.ProgressPercentage;
                var now = DateTime.UtcNow;
                // 检查距离上次报告是否已超过设定的时间间隔
                if (now - lastReportTime >= throttlePeriod || percentage >= 100) {
                    lastReportTime = now;
                    downloadProgress?.Invoke(percentage);
                }
            };

            // 确保目标目录存在
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory)) {
                Directory.CreateDirectory(directory);
            }

            var package = new DownloadPackage {
                FileName = destinationPath
            };

            await downloader.DownloadFileTaskAsync(package, url);
            return true;
        } catch (TaskCanceledException) {
            Log.Information("Download canceled: {0}", url);
            throw;
        } catch (Exception ex) {
            Log.Error("Download failed for {0}\n{1}", url, ex);
            throw;
        }
    }

    /**
     * 更新文件
     * @param url 下载地址
     * @param path 保存路径
     * @param name 下载名称
     */
    public static async Task DownloadAsync(string url, string path, string name)
    {
        // 下载插件 进度条 初始化
        var progressBar = new SyncProgressBarUtil.ProgressBar();
        // 下载插件 进度条 回调
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update => progressBar.Update(update.Percent, update.Message));
        await DownloadAsync(url, path, name, uiProgress);
    }

    /**
     * 更新文件
     * @param url 下载地址
     * @param path 保存路径
     * @param name 下载名称
     */
    private static async Task DownloadAsync(string url, string path, string name, SyncCallback<SyncProgressBarUtil.ProgressReport> progress)
    {
        await DownloadAsync(url, path, dp => {
            progress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = dp,
                Message = $"Downloading {name}"
            });
        });
    }
    
}