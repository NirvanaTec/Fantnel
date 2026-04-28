using Downloader;
using Nirvana.Game.Launcher.Utils.Progress;
using Serilog;

namespace Nirvana.Game.Launcher.Utils;

public static class DownloadUtil {
    public static async Task<bool> DownloadAsync(string url, string destinationPath, Action<double>? downloadProgress = null, int maxConcurrentSegments = 4)
    {
        try {
            var downloadOpt = new DownloadConfiguration {
                ChunkCount = maxConcurrentSegments, // 设置并发块数
                MaxTryAgainOnFailure = 3, // 下载失败后重试次数
                ParallelDownload = true, // 启用并行下载
                EnableAutoResumeDownload = true // 启用自动续传功能
            };

            await using var downloader = new DownloadService(downloadOpt);

            var lastReportTime = DateTime.MinValue;
            var throttlePeriod = TimeSpan.FromMilliseconds(200); // 设置更新间隔为100毫秒

            // 注册进度更新事件
            downloader.DownloadProgressChanged += (_, e) => {
                var now = DateTime.UtcNow;
                // 检查距离上次报告是否已超过设定的时间间隔
                if (now - lastReportTime >= throttlePeriod) {
                    lastReportTime = now;
                    downloadProgress?.Invoke(e.ProgressPercentage);
                }
            };

            downloader.DownloadFileCompleted += (_, e) => { downloadProgress?.Invoke(100); };

            // 分离目标路径为目标文件夹和文件名
            var fileInfo = new FileInfo(destinationPath);
            var directory = fileInfo.DirectoryName;

            // 确保目标目录存在
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
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
    public static async Task DownloadAsync(string url, string path, string name, SyncCallback<SyncProgressBarUtil.ProgressReport>? progress = null)
    {
        var uiProgress = progress;
        if (uiProgress == null) {
            // 下载插件 进度条 初始化
            var progressBar = new SyncProgressBarUtil.ProgressBar();
            // 下载插件 进度条 回调
            uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(progressBar.Update);
        }

        await DownloadAsync(url, path, dp => {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = dp,
                Message = $"Downloading {name}"
            });
        });
    }
}