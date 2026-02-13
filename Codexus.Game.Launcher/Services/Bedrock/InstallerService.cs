using System.Security.Cryptography;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using NirvanaAPI.Utils;

namespace Codexus.Game.Launcher.Services.Bedrock;

public static class InstallerService {
    public static async Task<bool> DownloadMinecraftAsync()
    {
        try {
            var paths = GetInstallationPaths();
            if (IsMinecraftInstalledAsync(paths).GetAwaiter().GetResult()) return true;
            FileUtil.CleanDirectorySafe(paths.BasePath);
            using var progressBar = new SyncProgressBarUtil.ProgressBar();
            var progress = CreateProgressReporter(progressBar);
            if (!await DownloadMinecraftPackage(paths.ArchivePath, progress)) return false;
            if (!await ValidateDownloadedPackage(paths.ArchivePath, progress)) return false;
            await ExtractMinecraftPackage(paths.ArchivePath, paths.BasePath, progress);
            await SavePackageHash(paths.ArchivePath, paths.HashPath);
            await CleanupTemporaryFiles(paths.ArchivePath, progress);
            return true;
        } catch (Exception innerException) {
            throw new InvalidOperationException("Failed to download and install Minecraft", innerException);
        } finally {
            SyncProgressBarUtil.ProgressBar.ClearCurrent();
        }
    }

    private static (string BasePath, string ArchivePath, string HashPath, string ExecutablePath) GetInstallationPaths()
    {
        var cppGamePath = PathUtil.CppGamePath;
        return (BasePath: cppGamePath, ArchivePath: Path.Combine(cppGamePath, "mc_base.7z"),
            HashPath: Path.Combine(cppGamePath, "minecraft_pe.md5"),
            ExecutablePath: Path.Combine(cppGamePath, "windowsmc", "Minecraft.Windows.exe"));
    }

    private static async Task<bool> IsMinecraftInstalledAsync(
        (string BasePath, string ArchivePath, string HashPath, string ExecutablePath) paths)
    {
        if (!File.Exists(paths.ExecutablePath) || !File.Exists(paths.HashPath)) return false;
        try {
            return string.Equals(Convert.ToHexStringLower(await File.ReadAllBytesAsync(paths.HashPath)),
                "50ac5016023c295222b979565b9c707b", StringComparison.OrdinalIgnoreCase);
        } catch (Exception) {
            return false;
        }
    }

    private static Progress<SyncProgressBarUtil.ProgressReport> CreateProgressReporter(
        SyncProgressBarUtil.ProgressBar progressBar)
    {
        return new Progress<SyncProgressBarUtil.ProgressReport>(update => {
            progressBar.Update(update.Percent, update.Message);
        });
    }

    private static async Task<bool> DownloadMinecraftPackage(string archivePath,
        IProgress<SyncProgressBarUtil.ProgressReport> progress)
    {
        return await DownloadUtil.DownloadAsync(
            "https://x19.gdl.netease.com/release2_20250402_3.4.Win32.Netease.r20.OGL.Publish_3.4.5.273310_20250627104722.7z",
            archivePath, percentage => {
                progress.Report(new SyncProgressBarUtil.ProgressReport {
                    Percent = percentage,
                    Message = "Downloading Minecraft Bedrock"
                });
            });
    }

    private static async Task<bool> ValidateDownloadedPackage(string archivePath,
        IProgress<SyncProgressBarUtil.ProgressReport> progress)
    {
        progress.Report(new SyncProgressBarUtil.ProgressReport {
            Percent = 0,
            Message = "Checking package validation"
        });
        if (!await ValidatePackageAsync(archivePath, "50ac5016023c295222b979565b9c707b")) return false;
        progress.Report(new SyncProgressBarUtil.ProgressReport {
            Percent = 100,
            Message = "Package validation succeeded"
        });
        return true;
    }

    private static async Task ExtractMinecraftPackage(string archivePath, string basePath,
        IProgress<SyncProgressBarUtil.ProgressReport> progress)
    {
        await CompressionUtil.ExtractAsync(archivePath, basePath, percentage => {
            progress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = percentage,
                Message = "Extracting Minecraft Bedrock"
            });
        });
    }

    private static async Task SavePackageHash(string archivePath, string hashPath)
    {
        var bytes = MD5.HashData(await File.ReadAllBytesAsync(archivePath));
        await File.WriteAllBytesAsync(hashPath, bytes);
    }

    private static async Task CleanupTemporaryFiles(string archivePath,
        IProgress<SyncProgressBarUtil.ProgressReport> progress)
    {
        progress.Report(new SyncProgressBarUtil.ProgressReport {
            Percent = 0,
            Message = "Cleaning up game resources"
        });
        FileUtil.DeleteFileSafe(archivePath);
        progress.Report(new SyncProgressBarUtil.ProgressReport {
            Percent = 100,
            Message = "Cleanup completed"
        });
        await Task.CompletedTask;
    }

    private static async Task<bool> ValidatePackageAsync(string filePath, string expectedMd5)
    {
        if (string.IsNullOrWhiteSpace(expectedMd5))
            throw new ArgumentException("Expected MD5 hash cannot be null or empty", nameof(expectedMd5));
        if (!File.Exists(filePath)) throw new FileNotFoundException("File not found: " + filePath, filePath);
        return string.Equals(Convert.ToHexStringLower(MD5.HashData(await File.ReadAllBytesAsync(filePath))),
            expectedMd5, StringComparison.OrdinalIgnoreCase);
    }
}