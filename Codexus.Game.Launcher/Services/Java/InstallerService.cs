using System.Security.Cryptography;
using System.Text.Json;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using NirvanaAPI.Utils;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils;

namespace Codexus.Game.Launcher.Services.Java;

public static class InstallerService {
    public static async Task PrepareMinecraftClient(EnumGameVersion gameVersion)
    {
        var versionName = Enum.GetName(gameVersion);

        var md5Path = Path.Combine(PathUtil.GameBasePath, "GAME_BASE.MD5");
        var zipPath = Path.Combine(PathUtil.CachePath, "GameBase.zip");

        var minecraftClientLibs = await WPFLauncher.GetMinecraftClientLibsAsync();
        await ProcessPackage(minecraftClientLibs.Url, zipPath, PathUtil.GameBasePath, md5Path, minecraftClientLibs.Md5,
            "base package");

        var versionMd5File = Path.Combine(PathUtil.GameBasePath, versionName + ".MD5");
        var versionZip = Path.Combine(PathUtil.CachePath, versionName + ".zip");

        var versionResult = await WPFLauncher.GetMinecraftClientLibsAsync(gameVersion);
        await ProcessPackage(versionResult.Url, versionZip, PathUtil.GameBasePath, versionMd5File, versionResult.Md5,
            versionName + " package");

        var libMd5File = Path.Combine(PathUtil.GameBasePath, versionName + "_Lib.MD5");
        var libZip = Path.Combine(PathUtil.CachePath, versionName + "_Lib.7z");

        await ProcessPackage(versionResult.CoreLibUrl, libZip, PathUtil.CachePath, libMd5File, versionResult.CoreLibMd5,
            versionName + " libraries");
        InstallCoreLibs(Path.Combine(PathUtil.CachePath, versionName + "_libs"), gameVersion);
    }

    private static async Task ProcessPackage(string url, string zipPath, string extractTo, string md5Path, string md5,
        string label)
    {
        // 已经下载过，且md5匹配，直接返回
        if (File.Exists(md5Path) && await File.ReadAllTextAsync(md5Path) == md5) return;

        var progress = new SyncProgressBarUtil.ProgressBar();
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update => {
            progress.Update(update.Percent, update.Message);
        });
        await DownloadUtil.DownloadAsync(url, zipPath, p => {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = p,
                Message = "Downloading " + label
            });
        });

        await CompressionUtil.ExtractAsync(zipPath, extractTo, p => {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = p,
                Message = "Extracting " + label
            });
        });

        if (md5Path != null) await File.WriteAllTextAsync(md5Path, md5);
        FileUtil.DeleteFileSafe(zipPath);
    }

    private static void InstallCoreLibs(string libPath, EnumGameVersion gameVersion)
    {
        var gameVersionFromEnum = GameVersionUtil.GetGameVersionFromEnum(gameVersion);
        var text = "forge-" + gameVersionFromEnum + "-";
        var text2 = "launchwrapper-";
        var text3 = "MercuriusUpdater-";
        var text4 = gameVersionFromEnum + ".jar";
        var text5 = gameVersionFromEnum + ".json";
        if (!Directory.Exists(libPath)) return;
        var files = Directory.GetFiles(libPath, "*", SearchOption.AllDirectories);
        foreach (var text6 in files) {
            var fileName = Path.GetFileName(text6);
            if (fileName.StartsWith(text)) {
                text = Path.GetFileNameWithoutExtension(text6);
                var path = text.Replace("forge-", "");
                var text7 = Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries", "net", "minecraftforge",
                    "forge",
                    path);
                var text8 = Path.Combine(text7, text + ".jar");
                if (!Directory.Exists(text7))
                    Directory.CreateDirectory(text7);
                else if (File.Exists(text8)) File.Delete(text8);
                File.Copy(text6, text8, true);
            } else if (fileName.StartsWith(text2)) {
                text2 = Path.GetFileNameWithoutExtension(text6);
                var path2 = text2.Replace("launchwrapper-", "");
                var text9 = Path.Combine(PathUtil.GameBasePath, ".minecraft",
                    "libraries", "net", "minecraft", "launchwrapper", path2);
                var text10 = Path.Combine(text9, text2 + ".jar");
                if (!Directory.Exists(text9))
                    Directory.CreateDirectory(text9);
                else if (File.Exists(text10)) File.Delete(text10);
                File.Copy(text6, text10, true);
            } else if (fileName.StartsWith(text3)) {
                text3 = Path.GetFileNameWithoutExtension(text6);
                var path3 = text3.Replace("MercuriusUpdater-", "");
                var text11 = Path.Combine(PathUtil.GameBasePath, ".minecraft",
                    "libraries", "net", "minecraftforge", "MercuriusUpdater", path3);
                var text12 = Path.Combine(text11, text3 + ".jar");
                if (!Directory.Exists(text11))
                    Directory.CreateDirectory(text11);
                else if (File.Exists(text12)) File.Delete(text12);
                File.Copy(text6, text12, true);
            } else if (fileName.Equals(text4)) {
                var destFileName = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", gameVersionFromEnum,
                    text4);
                File.Copy(text6, destFileName, true);
            } else if (fileName.Equals(text5)) {
                var destFileName2 = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", gameVersionFromEnum,
                    text5);
                File.Copy(text6, destFileName2, true);
            } else if (fileName.StartsWith("modlauncher-") && fileName.Contains("9.1.0")) {
                var destFileName3 = Path.Combine(new[] {
                    PathUtil.GameBasePath,
                    ".minecraft",
                    "libraries", "cpw", "mods", "modlauncher", "9.1.0", "modlauncher-9.1.0.jar"
                });
                File.Copy(text6, destFileName3, true);
            } else if (fileName.StartsWith("modlauncher-") && fileName.Contains("10.0.9")) {
                var destFileName4 = Path.Combine(new[] {
                    PathUtil.GameBasePath,
                    ".minecraft",
                    "libraries", "cpw", "mods", "modlauncher", "10.0.9", "modlauncher-10.0.9.jar"
                });
                // 创建目录
                var directory = Path.GetDirectoryName(destFileName4);
                if (!Directory.Exists(directory) && directory != null)
                    Directory.CreateDirectory(directory);
                File.Copy(text6, destFileName4, true);
            } else if (fileName.StartsWith("modlauncher-") && fileName.Contains("10.2.1")) {
                var destFileName5 = Path.Combine(new[] {
                    PathUtil.GameBasePath,
                    ".minecraft",
                    "libraries", "net", "minecraftforge", "modlauncher", "10.2.1", "modlauncher-10.2.1.jar"
                });
                File.Copy(text6, destFileName5, true);
            }
        }

        FileUtil.DeleteDirectorySafe(libPath);
    }

    public static async Task<EntityModsList?> InstallGameMods(EnumGameVersion gameVersion, string gameId,
        bool isRental = false)
    {
        var entity = await WPFLauncher.GetGameCoreModListAsync(gameVersion, isRental);
        if (entity?.IidList == null) return null;

        var entities = await WPFLauncher.GetGameCoreModDetailsListAsync(entity.IidList);
        var modList = new EntityModsList();

        foreach (var entityComponentDownloadInfoResponse in entities)
        foreach (var subEntity in entityComponentDownloadInfoResponse.SubEntities)
            modList.Mods.Add(new EntityModsInfo {
                ModPath =
                    $"{entityComponentDownloadInfoResponse.ItemId}@{entityComponentDownloadInfoResponse.MTypeId}@0.jar",
                Id =
                    $"{entityComponentDownloadInfoResponse.ItemId}@{entityComponentDownloadInfoResponse.MTypeId}@0.jar",
                Iid = entityComponentDownloadInfoResponse.ItemId,
                Md5 = subEntity.JarMd5.ToUpper(),
                Name = "",
                Version = ""
            });

        var progress = new SyncProgressBarUtil.ProgressBar();
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update => {
            progress.Update(update.Percent, update.Message);
        });
        var corePath = Path.Combine(PathUtil.GameModsPath, gameId);
        if (Directory.Exists(corePath)) Directory.Delete(corePath, true);
        var idx = 0;
        foreach (var entityComponentDownloadInfoResponse2 in entities) {
            var i = idx;
            idx = i + 1;
            var fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension(entityComponentDownloadInfoResponse2.SubEntities[0].ResName);
            var jar = Path.Combine(corePath,
                $"{fileNameWithoutExtension}@{entityComponentDownloadInfoResponse2.MTypeId}@{entityComponentDownloadInfoResponse2.EntityId}.jar");
            var archive = Path.Combine(corePath, entityComponentDownloadInfoResponse2.SubEntities[0].ResName);
            var extractDir = Path.Combine(corePath, fileNameWithoutExtension);
            // 创建目录
            if (!Directory.Exists(extractDir))
                Directory.CreateDirectory(extractDir);
            if (File.Exists(jar) && FileUtil.ComputeMd5FromFile(jar)
                    .Equals(entityComponentDownloadInfoResponse2.SubEntities[0].JarMd5,
                        StringComparison.OrdinalIgnoreCase)) continue;
            await DownloadUtil.DownloadAsync(entityComponentDownloadInfoResponse2.SubEntities[0].ResUrl, archive,
                dp => {
                    uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                        Percent = dp,
                        Message = $"Downloading core mod {idx}/{entities.Length}"
                    });
                });
            var idx2 = idx;
            await CompressionUtil.ExtractAsync(archive, extractDir, p => {
                uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                    Percent = p,
                    Message = $"Extracting core mod {idx2}/{entities.Length}"
                });
            });
            var array = FileUtil.EnumerateFiles(extractDir, "jar");
            for (i = 0; i < array.Length; i++) FileUtil.CopyFileSafe(array[i], jar);
            FileUtil.DeleteDirectorySafe(extractDir);
            FileUtil.DeleteFileSafe(archive);
        }

        uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
            Percent = 100,
            Message = "Core mods ready"
        });
        var compDir = Path.Combine(PathUtil.CachePath, "Game", gameId);
        var compArchive = compDir + ".7z";
        Directory.CreateDirectory(compDir);

        try {
            var netGameComponentDownloadList = await WPFLauncher.GetNetGameComponentDownloadListAsync(gameId);
            var comp = netGameComponentDownloadList.SubEntities[0];
            var extractDir = Path.Combine(compDir, gameId + ".MD5");
            var archive = Path.Combine(compDir, gameId + ".json");
            var flag = !File.Exists(extractDir);
            if (!flag) flag = await File.ReadAllTextAsync(extractDir) != comp.ResMd5;
            if (!flag && File.Exists(archive)) {
                var entityModsInfos = JsonSerializer.Deserialize<EntityModsList>(await File.ReadAllTextAsync(archive))
                    ?.Mods;
                if (entityModsInfos != null) {
                    foreach (var mod in entityModsInfos) {
                        modList.Mods.Add(mod);
                    }
                }

                uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                    Percent = 100,
                    Message = "Game assets ready"
                });
                SyncProgressBarUtil.ProgressBar.ClearCurrent();
                return modList;
            }

            FileUtil.DeleteFileSafe(compArchive);
            await DownloadUtil.DownloadAsync(comp.ResUrl, compArchive, p => {
                uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                    Percent = p,
                    Message = "Downloading game assets"
                });
            });
            FileUtil.DeleteDirectorySafe(compDir);
            await CompressionUtil.ExtractAsync(compArchive, compDir, p => {
                uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                    Percent = p,
                    Message = "Extracting game assets"
                });
            });
            var array2 = FileUtil.EnumerateFiles(Path.Combine(compDir, ".minecraft", "mods"), "jar");
            var serverModsList = new EntityModsList();
            foreach (var path in array2) {
                var jar = Path.GetFileName(path);
                var md = Convert.ToHexString(MD5.HashData(await File.ReadAllBytesAsync(path))).ToUpper();
                serverModsList.Mods.Add(new EntityModsInfo {
                    Name = "",
                    Version = "",
                    ModPath = jar,
                    Id = jar,
                    Iid = jar.Split('@')[0],
                    Md5 = md
                });
            }

            modList.Mods.AddRange(serverModsList.Mods);
            await File.WriteAllTextAsync(extractDir, comp.ResMd5);
            await File.WriteAllTextAsync(archive, JsonSerializer.Serialize(serverModsList));
            FileUtil.DeleteFileSafe(compArchive);
        } catch (Exception) {
            Log.Warning("Download game Component failed");
        }

        uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
            Percent = 100,
            Message = "Game assets ready"
        });
        SyncProgressBarUtil.ProgressBar.ClearCurrent();
        return modList;
    }

    private static void InstallCustomMods(string mods)
    {
        FileUtil.EnumerateFiles(PathUtil.CustomModsPath, "jar").ToList().ForEach(f => {
            FileUtil.CopyFileSafe(f, Path.Combine(mods, Path.GetFileName(f)));
        });
    }

    public static string PrepareGameRuntime(string gameId, string roleName, EnumGType gameType)
    {
        var path = HashUtil.GenerateGameRuntimeId(gameId, roleName);
        var text = Path.Combine(PathUtil.GamePath, "Runtime", path);
        var text2 = Path.Combine(text, ".minecraft");

        Directory.CreateDirectory(text);
        Directory.CreateDirectory(text2);

        if (gameType == EnumGType.NetGame) {
            var text3 = Path.Combine(text2, "mods");
            FileUtil.DeleteDirectorySafe(text3);
            Directory.CreateDirectory(text3);
            FileUtil.CopyDirectory(Path.Combine(PathUtil.CachePath, "Game", gameId, ".minecraft"), text2, false);
            InstallCustomMods(text3);
        }

        var linkPath = Path.Combine(text2, "assets");
        var targetPath = Path.Combine(PathUtil.GameBasePath, ".minecraft", "assets");
        // 创建assets目录符号链接
        if (Directory.Exists(linkPath)) Directory.Delete(linkPath);
        Directory.CreateSymbolicLink(linkPath, targetPath + Path.DirectorySeparatorChar);
        return text;
    }

    public static void InstallCoreMods(string gameId, string targetModsPath)
    {
        var text = Path.Combine(PathUtil.GameModsPath, gameId);
        if (!Directory.Exists(text)) return;
        Directory.CreateDirectory(targetModsPath);
        var array = FileUtil.EnumerateFiles(text);
        foreach (var text2 in array) {
            var text3 = Path.Combine(targetModsPath, Path.GetRelativePath(text, text2));
            var dir = Path.GetDirectoryName(text3);
            if (dir == null) continue;
            Directory.CreateDirectory(dir);
            FileUtil.CopyFileSafe(text2, text3);
        }
    }

    public static void InstallNativeDll(EnumGameVersion gameVersion)
    {
        try {
            var text = Path.Combine(PathUtil.ResourcePath, "api-ms-win-crt-utility-l1-1-1.dll");
            var text2 = Path.Combine(
                PathUtil.GameBasePath,
                ".minecraft",
                "versions",
                GameVersionUtil.GetGameVersionFromEnum(gameVersion),
                "natives",
                "runtime"
            );
            Directory.CreateDirectory(text2);
            if (!File.Exists(text)) throw new Exception("未找到验证库: " + text);
            var destPath = Path.Combine(text2, "api-ms-win-crt-utility-l1-1-1.dll");
            FileUtil.CopyFileSafe(text, destPath);
        } catch (Exception ex) {
            Log.Error("Failed to install native dll:{ex}", ex);
        }
    }
}