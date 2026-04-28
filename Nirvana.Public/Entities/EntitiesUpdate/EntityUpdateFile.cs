using System.Text.Json.Nodes;
using Nirvana.Public.Utils.Update;
using NirvanaAPI.Utils;

namespace Nirvana.Public.Entities.EntitiesUpdate;

public class EntityUpdateFile {
    private static readonly Lock Lock = new();
    private static int _downloadCountInSecond; // 线程下载数
    private readonly string? _downloadUrl; // 文件下载地址
    private readonly string? _fileSha256; // 文件SHA256
    private readonly long? _fileSize; // 文件大小
    private readonly string? _pathValue; // 文件基础路径

    public required int Index;

    public EntityUpdateFile(JsonNode? item)
    {
        // 文件下载地址
        var url = item?["url"];
        if (url != null) {
            _downloadUrl = url.GetValue<string>();
        }

        // 文件大小
        var size = item?["size"];
        if (size != null) {
            _fileSize = size.GetValue<long>();
        }

        // 文件SHA256
        var sha256 = item?["sha256"];
        if (sha256 != null) {
            _fileSha256 = sha256.GetValue<string>();
        }

        // 文件基础路径
        var path = item?["path"];
        if (path != null) {
            _pathValue = path.GetValue<string>().Replace('\\', Path.DirectorySeparatorChar);
        }
    }

    /**
     * 检查单个文件更新
     * @param filePath 完整文件路径
     */
    public async Task<int> CheckUpdate(string filePath, string safeSavePath, Action<double>? downloadProgress = null)
    {
        if (string.IsNullOrEmpty(_downloadUrl)) {
            return 2;
        }

        // 硬盘访问速限制 1 秒 / 60次 ≈ 0.016
        lock (Lock) {
            Thread.Sleep(16);
        }

        // 检查是否需要更新
        if (!NeedsUpdate(filePath)) {
            return 0;
        }

        // 请求速限制 1 秒 / 9次 ≈ 0.111
        lock (Lock) {
            // 111 - 16 = 95ms
            Thread.Sleep(95);
        }

        // 下载频率限制
        await WaitForDownloadRateLimitAsync();
        // 下载文件
        var success = await DownloadUtil.DownloadAsync(_downloadUrl, safeSavePath, progressValue => { downloadProgress?.Invoke(progressValue); });
        lock (Lock) {
            _downloadCountInSecond--;
        }

        return success ? 1 : 3;
    }

    private static async Task WaitForDownloadRateLimitAsync()
    {
        while (true) {
            lock (Lock) {
                // 未达到上限，允许下载
                if (_downloadCountInSecond < 3) {
                    _downloadCountInSecond++;
                    break;
                }
            }

            // 达到上限，等待一小段时间再重试
            await Task.Delay(100);
        }
    }

    private bool NeedsUpdate(string filePath)
    {
        // 文件不存在
        if (!File.Exists(filePath)) {
            return true;
        }

        // 文件大小不同
        if (_fileSize != null) {
            var actualSize = new FileInfo(filePath).Length;
            if (actualSize != _fileSize) {
                return true;
            }
        }

        // 检查SHA256
        if (_fileSha256 == null) {
            return false;
        }

        var actualHash = Tools.ComputeSha256(filePath);
        return !string.Equals(actualHash, _fileSha256, StringComparison.OrdinalIgnoreCase);
    }

    public string? GetPath(bool safeMode, params string[] basePathList)
    {
        if (string.IsNullOrEmpty(_pathValue)) {
            return null;
        }

        var list = new List<string> {
            safeMode ? PathUtil.UpdaterPath : PathUtil.UpdaterBasePath
        };
        list.AddRange(basePathList);
        list.Add(_pathValue);
        return Path.Combine(list.ToArray());
    }
}