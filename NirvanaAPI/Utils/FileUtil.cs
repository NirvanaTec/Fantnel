using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Serilog;

namespace NirvanaAPI.Utils;

public static class FileUtil {
    public static string[] EnumerateFiles(string path, string? fileType = null)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) {
            return [];
        }

        var searchPattern = string.IsNullOrWhiteSpace(fileType) ? "*" : "*." + fileType.TrimStart('.').ToLowerInvariant();
        return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).ToArray();
    }

    public static string ComputeMd5FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) {
            return string.Empty;
        }
        try {
            using var inputStream = File.OpenRead(path);
            using var mD = MD5.Create();
            return Convert.ToHexString(mD.ComputeHash(inputStream)).ToLowerInvariant();
        } catch (IOException) {
            return string.Empty;
        } catch (UnauthorizedAccessException) {
            return string.Empty;
        }
    }

    public static void CleanDirectorySafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) {
            return;
        }
        DeleteDirectorySafe(path);
        Directory.CreateDirectory(path);
    }

    public static void DeleteDirectorySafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) {
            return;
        }
        try {
            Directory.Delete(path, true);
        } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    public static bool CopyFileSafe(string sourcePath, string destPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destPath)) {
            return false;
        }

        try {
            var directoryName = Path.GetDirectoryName(destPath);
            if (string.IsNullOrEmpty(directoryName)) {
                return false;
            }

            Directory.CreateDirectory(directoryName);
            File.Copy(sourcePath, destPath, true);
            return true;
        } catch (IOException) {
            return false;
        } catch (UnauthorizedAccessException) {
            return false;
        }
    }

    public static void CopyDirectory(string sourceDir, string targetDir, bool includeRoot = true, bool deleteSource = false)
    {
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir)) {
            return;
        }

        var name = new DirectoryInfo(sourceDir).Name;
        var text = includeRoot ? Path.Combine(targetDir, name) : targetDir;
        Directory.CreateDirectory(text);
        foreach (var item in Directory.EnumerateFileSystemEntries(sourceDir)) {
            var fileName = Path.GetFileName(item);
            var destFileName = Path.Combine(text, fileName);
            if (Directory.Exists(item)) {
                CopyDirectory(item, text, true, deleteSource);
                if (deleteSource) {
                    Directory.Delete(item, true);
                }
            } else {
                File.Copy(item, destFileName, true);
                if (deleteSource) {
                    File.Delete(item);
                }
            }
        }
    }

    public static bool DeleteFileSafe(string path)
    {
        try {
            if (!File.Exists(path)) {
                return true;
            }

            File.Delete(path);
            return true;
        } catch (Exception) {
            return false;
        }
    }

    /**
     * 设置文件权限
     * @param filePath 文件路径
     * @param requiredPermissions 所需权限，默认所有权限
     */
    public static void SetUnixFilePermissions(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return;
        }

        try {
            var processStartInfo = new ProcessStartInfo("chmod", $"755 \"{filePath}\"");
            var process = Process.Start(processStartInfo);
            process?.WaitForExit();
        } catch (Exception e) {
            Log.Warning("警告：使用 chmod 设置 {0} 权限时出错: {1}", filePath, e.Message);
        }
    }
}