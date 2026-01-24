using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Codexus.Game.Launcher.Utils;

public static class CompressionUtil
{

    private static async Task ExtractZipAsync(string archivePath, string outPath,
        Action<int> progress = null)
    {
        try
        {
            // 1. 打开压缩包并获取所有条目
            using var archive = ArchiveFactory.OpenArchive(archivePath);

            // 创建一个线程安全的队列来存放待处理的条目
            var entriesQueue = new ConcurrentQueue<IArchiveEntry>();

            // 只处理非目录条目
            var allEntries = archive.Entries.Where(entry => !entry.IsDirectory).ToList();
            var totalEntries = allEntries.Count;
            foreach (var entry in allEntries) entriesQueue.Enqueue(entry);
            if (totalEntries == 0)
            {
                progress?.Invoke(100);
                return; // 没有文件需要解压
            }

            // 初始化进度状态
            var progressState = new ProgressState
            {
                TotalEntries = totalEntries
            };

            await ProcessEntriesFromQueueAsync(entriesQueue, outPath, progress, progressState);
        }
        catch (Exception e)
        {
            Log.Fatal("解压时出错: {filePath}", archivePath);
            Log.Fatal("解压错误：{message}", e.Message);
            throw;
        }
    }

    public static async Task ExtractAsync(string archivePath, string outPath,
        Action<int> progress = null)
    {
        if (Is7ZipFormat(archivePath))
            await Extract7ZAsync(archivePath, outPath, progress);
        else
            await ExtractZipAsync(archivePath, outPath, progress);
    }

    /**
     * @Return 是否为7z格式
     */
    private static bool Is7ZipFormat(string archivePath)
    {
        using var stream = File.OpenRead(archivePath);
        using var archive = ArchiveFactory.OpenArchive(stream);
        return archive.Type == ArchiveType.SevenZip;
    }

    private static async Task Extract7ZAsync(string archivePath, string outPath, Action<int> progress = null)
    {
        // 取文件信息
        var fileInfo = new FileInfo(archivePath);
        // 超过 30mb
        if (fileInfo.Length > 30 * 1024 * 1024)
        {
            if (Extract7Z_7ZIP(archivePath, outPath, progress)) return;
            Log.Warning("使用通用模式解压7z文件中....");
            Log.Warning("Path: {archivePath}", archivePath);
        }

        await ExtractZipAsync(archivePath, outPath, progress);
    }

    /**
     * 单文件解压
     * @return 是否成功
     */
    private static bool Extract7Z_7ZIP(string archivePath, string outputDirectory, Action<int> progress = null)
    {
        if (!File.Exists(archivePath))
        {
            Log.Error("错误：压缩包文件不存在 - {ArchivePath}", archivePath);
            return false;
        }

        try
        {
            var sevenZipExePath = Get7ZipPath();

            Directory.CreateDirectory(outputDirectory);

            using var process = new Process();

            process.StartInfo.WorkingDirectory = outputDirectory;
            process.StartInfo.FileName = sevenZipExePath;
            process.StartInfo.Arguments = $"x -y \"{archivePath}\"";

            process.Start(); // 启动进程
            process.WaitForExit(); // 等待进程退出
            progress?.Invoke(100);

            // 7-Zip 成功时退出码通常为 0。
            // 1: 警告 (例如，有些文件被锁定)，2: 错误，其他值可能表示不同错误。
            return process.ExitCode is 0 or 1; // 根据实际需求判断成功条件
        }
        catch (Exception ex)
        {
            Log.Error("解压过程中发生异常: \n{ExMessage}", ex.Message);
            return false;
        }
    }

    private static string Get7ZipPath()
    {
        var path = Path.Combine(PathUtil.ResourcePath,
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "7z.exe" : "7zz");
        return File.Exists(path) ? path : throw new Exception("未找到 7-Zip 可执行文件");
    }

    /**
     * 循环处理队列中的压缩条目
     * @entriesQueue 待处理的条目队列
     * @destinationPath 解压目标路径
     */
    private static async Task ProcessEntriesFromQueueAsync(
        ConcurrentQueue<IArchiveEntry> entriesQueue,
        string destinationPath,
        Action<int> progress,
        ProgressState progressState) // 传入封装了状态的对象
    {
        while (entriesQueue.TryDequeue(out var entry))
        {
            try
            {
                // 5. 执行单个文件的解压和写入操作
                await ExtractSingleEntryAsync(entry, destinationPath);
            }
            finally
            {
                // 更新进度 - 必须在锁内进行
                lock (progressState.Lock)
                {
                    progressState.ProcessedCount++;
                    var currentPercentage = progressState.ProcessedCount * 100 / progressState.TotalEntries;
                    progress?.Invoke(currentPercentage);
                }
            }
        }
    }

    /**
     * 解压单个条目到目标路径
     * @entry 要解压的条目
     * @destinationPath 解压目标路径
     */
    private static async Task ExtractSingleEntryAsync(IArchiveEntry entry, string destinationPath)
    {
        if (entry.Key == null) return;

        // 计算目标文件的完整路径
        var fullPath = Path.Combine(destinationPath, entry.Key.Replace('/', Path.DirectorySeparatorChar));

        // 确保目标文件的目录存在
        var dirPath = Path.GetDirectoryName(fullPath);
        if (dirPath == null) return;
        Directory.CreateDirectory(dirPath);

        // 使用 WriteEntryToFile 方法进行解压
        // 注意：WriteEntryToFile 在 SharpCompress v0.37+ 版本中是同步的。
        // 虽然我们不能直接异步写入单个文件（SharpCompress API 本身未提供异步写入单个文件的方法），
        // 但我们通过并发处理多个 *不同* 的文件条目来实现整体上的多线程效果。
        // SemaphoreSlim 在这里起到了控制并发文件写入数量的作用。
        await entry.WriteToFileAsync(fullPath, new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = true // 根据需要决定是否覆盖现有文件
        });
    }

    private class ProgressState
    {
        public readonly object Lock = new();
        public int ProcessedCount;
        public int TotalEntries; // 添加总条目数作为字段
    }
}