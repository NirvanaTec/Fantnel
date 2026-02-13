using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Net.Http.Headers;
using Codexus.Game.Launcher.Utils.Progress;
using Serilog;

namespace Codexus.Game.Launcher.Utils;

public static class DownloadUtil {
    private static readonly HttpClient HttpClient;

    static DownloadUtil()
    {
        HttpClient = new HttpClient(new HttpClientHandler {
            MaxConnectionsPerServer = 16
        }) {
            Timeout = TimeSpan.FromMinutes(10L)
        };
    }

    public static async Task<bool> DownloadAsync(string url, string destinationPath,
        Action<int>? downloadProgress = null, int maxConcurrentSegments = 8,
        CancellationToken cancellationToken = default)
    {
        long totalSize;
        long totalRead;
        Stopwatch stopwatch;
        int lastReportedProgress;
        try {
            var directoryName = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            using var headReq = new HttpRequestMessage(HttpMethod.Head, url);
            using var headResp =
                await HttpClient.SendAsync(headReq, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            headResp.EnsureSuccessStatusCode();
            var contentLength = headResp.Content.Headers.ContentLength;
            if (!contentLength.HasValue)
                return await SingleDownloadAsync(url, destinationPath, downloadProgress, cancellationToken);
            totalSize = contentLength.GetValueOrDefault();
            if (!headResp.Headers.AcceptRanges.Contains("bytes") || maxConcurrentSegments < 2 ||
                totalSize < 1048576)
                return await SingleDownloadAsync(url, destinationPath, downloadProgress, cancellationToken);
            var mmFile = MemoryMappedFile.CreateFromFile(destinationPath, FileMode.Create, null, totalSize,
                MemoryMappedFileAccess.ReadWrite);
            try {
                var errors = new ConcurrentBag<Exception>();
                IEnumerable<(long, long)> source = CalculateRanges(maxConcurrentSegments * 3, totalSize);
                totalRead = 0L;
                stopwatch = Stopwatch.StartNew();
                lastReportedProgress = -1;
                var semaphore = new SemaphoreSlim(maxConcurrentSegments, maxConcurrentSegments);
                try {
                    await Task.WhenAll(source.Select(async ((long Start, long End) range) => {
                        // ReSharper disable once AccessToDisposedClosure
                        await semaphore.WaitAsync(cancellationToken);
                        try {
                            for (var attempt = 1; attempt <= 3; attempt++) {
                                cancellationToken.ThrowIfCancellationRequested();
                                try {
                                    using var req = new HttpRequestMessage(HttpMethod.Get, url);
                                    req.Headers.Range = new RangeHeaderValue(range.Start, range.End);
                                    using var resp = await HttpClient.SendAsync(req,
                                        HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                                    resp.EnsureSuccessStatusCode();
                                    await using var netStream =
                                        await resp.Content.ReadAsStreamAsync(cancellationToken);
                                    // ReSharper disable once AccessToDisposedClosure
                                    await using var viewStream = mmFile.CreateViewStream(range.Start,
                                        range.End - range.Start + 1,
                                        MemoryMappedFileAccess.Write);
                                    var buffer = new byte[8192];
                                    while (true) {
                                        int num;
                                        var bytesRead = num =
                                            await netStream.ReadAsync(buffer, cancellationToken);
                                        if (num <= 0) break;
                                        await viewStream.WriteAsync(buffer.AsMemory(0, bytesRead),
                                            cancellationToken);
                                        ReportProgressThrottled(bytesRead);
                                    }

                                    break;
                                } catch (Exception ex2) when (attempt < 3 && ex2 is not OperationCanceledException) {
                                    await Task.Delay(500 * attempt, cancellationToken);
                                }
                            }
                        } catch (Exception item) {
                            errors.Add(item);
                        } finally {
                            // ReSharper disable once AccessToDisposedClosure
                            semaphore.Release();
                        }
                    }));
                    if (!errors.IsEmpty) throw new AggregateException(errors);
                    downloadProgress?.Invoke(100);
                    return true;
                } finally {
                    semaphore.Dispose();
                }
            } finally {
                mmFile.Dispose();
            }
            // return await SingleDownloadAsync(url, destinationPath, downloadProgress, cancellationToken);
        } catch (OperationCanceledException) {
            Log.Information("Download canceled: {Url}", url);
            throw;
        } catch (Exception exception) {
            Log.Error(exception, "Download failed for {Url}", url);
            return false;
        }

        void ReportProgressThrottled(long bytesRead)
        {
            var num = Interlocked.Add(ref totalRead, bytesRead);
            if (stopwatch.ElapsedMilliseconds <= 150) return;
            stopwatch.Restart();
            var num2 = (int)(num * 100.0 / totalSize);
            if (num2 <= lastReportedProgress) return;
            lastReportedProgress = num2;
            downloadProgress?.Invoke(num2);
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
        var progress = new SyncProgressBarUtil.ProgressBar();
        // 下载插件 进度条 回调
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update =>
            progress.Update(update.Percent, update.Message));

        await DownloadAsync(url, path, dp => {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = dp,
                Message = $"Downloading {name}"
            });
        });
    }

    private static async Task<bool> SingleDownloadAsync(string url, string destinationPath,
        Action<int>? downloadProgress, CancellationToken cancellationToken)
    {
        using var resp = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength.GetValueOrDefault();
        var read = 0L;
        await using var input = await resp.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None,
            8192, true);
        var buffer = new byte[8192];
        var stopwatch = Stopwatch.StartNew();
        var lastReportedProgress = -1;
        while (true) {
            int num;
            var n = num = await input.ReadAsync(buffer, cancellationToken);
            if (num <= 0) break;
            await output.WriteAsync(buffer.AsMemory(0, n), cancellationToken);
            if (total <= 0) continue;
            read += n;
            if (stopwatch.ElapsedMilliseconds <= 150) continue;
            stopwatch.Restart();
            var num2 = (int)(read * 100.0 / total);
            if (num2 <= lastReportedProgress) continue;
            lastReportedProgress = num2;
            downloadProgress?.Invoke(num2);
        }

        downloadProgress?.Invoke(100);
        return true;
    }

    private static IEnumerable<(long Start, long End)> CalculateRanges(int segments, long totalSize)
    {
        var segmentSize = totalSize / segments;
        for (var i = 0; i < segments; i++) {
            var item = i * segmentSize;
            var item2 = i == segments - 1 ? totalSize - 1 : (i + 1) * segmentSize - 1;
            yield return (Start: item, End: item2);
        }
    }
}