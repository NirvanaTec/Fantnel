using Nirvana.Game.Launcher.Entities;
using Nirvana.Game.Launcher.Services.Java;
using Nirvana.Game.Launcher.Utils;
using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Minecraft;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using Nirvana.WPFLauncher.Http;
using Nirvana.WPFLauncher.Protocol;
using Nirvana.WPFLauncher.Utils;
using NirvanaAPI.Entities;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Public.Message;

public static class LaunchMessage {
    // 启动游戏
    public static async Task<LauncherService> LaunchGame(string id, string name, string mode = "net")
    {
        if ("rental".Equals(mode)) {
            return await LaunchGameRental(id, name);
        }

        return await LaunchGameNet(id, name);
    }

    private static async Task<LauncherService> LaunchGameRental(string id, string name)
    {
        // 清理旧白端
        ActiveGameAndProxies.Close(id, name);

        Log.Information("正在启动白端游戏...");
        Log.Information("名称：{0}", name);

        // 服务器详细信息
        var server = await NPFLauncher.GetRentalGameDetailsAsync(id);

        // 服务器版本
        var versionName = server.McVersion; // 1.20
        var gameVersion = GameVersionUtil.GetEnumFromGameVersion(versionName);

        // 检测并自动安装java环境
        ExEnvironment(gameVersion);

        // 服务器角色信息
        var character = await RentalGameMessage.GetUserName(server.EntityId, name);
        if (character == null) {
            throw new ErrorCodeException(ErrorCode.NotFoundName);
        }

        // 服务器地址
        var address = await NPFLauncher.GetGameRentalAddressAsync(server.EntityId);

        var launchRequest = new EntityLaunchGame {
            Id = ActiveGameAndProxies.GetIndex(),
            GameName = server.ServerName,
            GameId = server.EntityId,
            RoleName = character.Name,
            ClientType = EnumGameClientType.Java,
            GameType = EnumGType.ServerGame,
            GameVersionId = (int)gameVersion,
            GameVersion = versionName,
            Account = InfoManager.GetGameAccount(),
            ServerIp = address.McServerHost,
            ServerPort = address.McServerPort,
            LoadCoreMods = true
        };

        // 启动白端游戏
        var launcherService = new LauncherService(launchRequest);
        await launcherService.LaunchGameAsync();
        ActiveGameAndProxies.Add(launcherService);
        return launcherService;
    }

    private static async Task<LauncherService> LaunchGameNet(string id, string name)
    {
        // 清理旧白端
        ActiveGameAndProxies.Close(id, name);

        Log.Information("正在启动白端游戏...");
        Log.Information("名称：{0}", name);

        // 服务器详细信息
        var server = await NPFLauncher.GetNetGameDetailByIdAsync(id);

        // 服务器版本
        var version = server.McVersionList[0]; // 1.20
        var gameVersion = GameVersionUtil.GetEnumFromGameVersion(version.Name);

        // 检测并自动安装java环境
        ExEnvironment(gameVersion);

        // 服务器角色信息
        var character = await ServersGameMessage.GetUserName(server.EntityId, name);
        if (character == null) {
            throw new ErrorCodeException(ErrorCode.NotFoundName);
        }

        // 服务器地址
        var address = await NPFLauncher.GetNetGameServerAddressAsync(server.EntityId);

        var launchRequest = new EntityLaunchGame {
            Id = ActiveGameAndProxies.GetIndex(),
            GameName = server.Name,
            GameId = server.EntityId,
            RoleName = character.Name,
            ClientType = EnumGameClientType.Java,
            GameType = EnumGType.NetGame,
            GameVersionId = (int)gameVersion,
            GameVersion = version.Name,
            Account = InfoManager.GetGameAccount(),
            ServerIp = address.Host,
            ServerPort = address.Port,
            LoadCoreMods = true
        };

        // 启动白端游戏
        var launcherService = new LauncherService(launchRequest);
        await launcherService.LaunchGameAsync();
        ActiveGameAndProxies.Add(launcherService);
        return launcherService;
    }

    private static void ExEnvironment(EnumGameVersion gameVersion)
    {
        ExEnvironmentByJava(gameVersion).Wait();
    }

    // 检测并自动安装java环境
    private static async Task ExEnvironmentByJava(EnumGameVersion gameVersion)
    {
        // 检查并安装Java环境
        string javaName;
        string javaPath;
        if (gameVersion >= EnumGameVersion.V_1_16) {
            javaName = "jdk17";
            javaPath = PathUtil.Jre17Path;
        } else {
            javaName = "jre8";
            javaPath = PathUtil.Jre8Path;
        }

        var id = PublicProgram.Mode + "." + PublicProgram.Arch + "." + javaName + ".java";
        var response = await X19Extensions.Nirvana.Api<EntityResponse<EntityMd5AndUrl>>("/api/fantnel/resource/md5?id=" + id);
        if (response?.Data == null) {
            throw new ErrorCodeException(ErrorCode.NotFound);
        }

        var md5File = Path.Combine(PathUtil.JavaPath, javaName + ".md5");
        if (File.Exists(md5File)) {
            var fileMd5 = await File.ReadAllTextAsync(md5File);
            if (fileMd5 == response.Data.Md5) {
                Log.Information("Java Path: {0}", javaPath);
                return;
            }
        }

        var filePath = Path.Combine(javaPath, javaName + ".zip");
        await DownloadUtil.DownloadAsync(response.Data.Url, filePath, javaName);
        await CompressionUtil.ExtractAsync(filePath, javaPath, javaName);
        await File.WriteAllTextAsync(md5File, response.Data.Md5);
        Log.Information("Java Path: {0}", javaPath);
    }
}