using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;

namespace Codexus.Game.Launcher.Utils;

public static class CompressionUtil
{
    public static void Extract7Z(string filePath, string outPath, Action<int> progressAction)
    {
        try
        {
            using var val = SevenZipArchive.Open(filePath);
            val.ExtractToDirectory(outPath, dp => { progressAction((int)(dp * 100.0)); });
        }
        catch (Exception e)
        {
            Log.Fatal("解压7z时出错: {filePath}", filePath);
            Log.Fatal("解压错误：{message}", e.Message);
        }
    }

    public static void ExtractZip(string filePath, string outPath, Action<int> progressAction)
    {
        try
        {
            using var val = ZipArchive.Open(filePath);
            val.ExtractToDirectory(outPath, dp => { progressAction((int)(dp * 100.0)); });
        }
        catch (Exception e)
        {
            Log.Fatal("解压zip时出错: {filePath}", filePath);
            Log.Fatal("解压错误：{message}", e.Message);
        }
    }

    public static async Task Extract7ZAsync(string archivePath, string outputDir,
        Action<int> progress = null,
        int maxDegreeOfParallelism = 12)
    {
        var processedCount = 0;
        var totalEntries = -1;
        var keys = Extract7ZAsync1(archivePath, outputDir, total =>
        {
            processedCount++;
            if (totalEntries == -1) totalEntries = total;
            progress?.Invoke((int)(processedCount / (double)total * 100));
        }, maxDegreeOfParallelism).Result;
        var totalEntries1 = -1;
        await Extract7ZAsync(archivePath, outputDir, total =>
        {
            processedCount++;
            if (totalEntries1 == -1) totalEntries1 = total;
            var total1 = totalEntries + totalEntries1;
            progress?.Invoke((int)(processedCount / (double)total1 * 100));
        }, keys);
    }

    private static async Task Extract7ZAsync(string archivePath, string outputDir, Action<int> progress,
        List<string> keys)
    {
        await Task.Run(() =>
        {
            using var val = ArchiveFactory.Open(archivePath);

            foreach (var entry in val.Entries)
            {
                // 跳过目录和空文件名
                if (entry.IsDirectory || entry.Key == null) continue;
                // 跳过不在 keys 中的文件
                if (!keys.Contains(entry.Key)) continue;
                var path = Path.Combine(outputDir, entry.Key);
                var directoryName = Path.GetDirectoryName(path);
                if (directoryName == null) throw new ArgumentException("Invalid directory name");
                if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
                using var stream = entry.OpenEntryStream();
                using var destination = File.Create(path);
                stream.CopyTo(destination);
                progress.Invoke(val.Entries.Count());
            }
        });
    }

    private static async Task<List<string>> Extract7ZAsync1(string archivePath, string outputDir,
        Action<int> progress,
        int maxDegreeOfParallelism = 12)
    {
        Directory.CreateDirectory(outputDir);

        using var archive = ArchiveFactory.Open(archivePath);
        var entries = archive.Entries.Where(e => !e.IsDirectory).ToList();
        var lockObj = new Lock();
        var totalEntries = archive.Entries.Count();
        // 7z 格式通常不适合完全并行解压，特别是固实压缩包
        // 更好的方法是并行处理文件写入，而非同时打开多个条目流

        var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

        try
        {
            List<string> throwList = [];
            await Task.Run(() => Parallel.ForEach(entries, options, entry =>
            {
                try
                {
                    // 安全处理文件路径，防止路径遍历
                    if (entry.Key != null)
                    {
                        var safeFileName =
                            Path.GetFileName(entry.Key.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        var relativePath = entry.Key[..^safeFileName.Length]
                            .Replace("/", Path.DirectorySeparatorChar.ToString());

                        var entryOutputDir = Path.Combine(outputDir, relativePath);
                        Directory.CreateDirectory(entryOutputDir);

                        var outputPath = Path.Combine(entryOutputDir, safeFileName);

                        // 每个线程单独打开流
                        using var entryStream = entry.OpenEntryStream();
                        using var fs = File.Create(outputPath);

                        // 使用缓冲提高性能
                        var buffer = new byte[8192];
                        int read;
                        while ((read = entryStream.Read(buffer, 0, buffer.Length)) > 0) fs.Write(buffer, 0, read);
                        fs.Flush();
                    }

                    // 报告进度
                    lock (lockObj)
                    {
                        progress?.Invoke(totalEntries);
                    }
                }
                catch (Exception)
                {
                    throwList.Add(entry.Key);
                }
            }));
            return throwList;
        }
        catch (Exception ex)
        {
            Log.Fatal("解压文件时出错: {archivePath}", archivePath);
            Log.Fatal("解压错误：{message}", ex.Message);
        }

        return [];
    }
}