using System.Runtime.InteropServices;
using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Services.Java;
using Codexus.Game.Launcher.Utils;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Message;

public static class LaunchMessage
{
    // 启动游戏
    public static async Task LaunchGame(string id, string name)
    {
        // 清理旧白端
        ActiveGameAndProxies.Close(id, name);

        Log.Information("正在启动白端游戏...");
        Log.Information("名称：{name}", name);

        // 服务器普通信息
        var server = ServersGameMessage.GetServerId(id);
        if (server == null) throw new ErrorCodeException(ErrorCode.ServerInNot);

        // 服务器详细信息
        var details = await WPFLauncher.QueryNetGameDetailByIdAsync(server.EntityId);

        // 服务器版本
        var version = details.McVersionList[0]; // 1.20
        var gameVersion = GameVersionUtil.GetEnumFromGameVersion(version.Name);

        // 检测并自动安装java环境
        ExEnvironment(gameVersion);

        // 安装模组
        await InstallerService.InstallGameMods(gameVersion, server.EntityId);

        // 服务器角色信息
        var character = await ServerInfoMessage.GetUserName(server.EntityId, name);
        if (character == null) throw new ErrorCodeException(ErrorCode.NotFoundName);

        // 服务器地址
        var address = await WPFLauncher.GetNetGameServerAddressAsync(server.EntityId);

        var launchRequest = new EntityLaunchGame
        {
            GameName = server.Name,
            GameId = server.EntityId,
            RoleName = character.Name,
            UserId = character.UserId,
            ClientType = EnumGameClientType.Java,
            GameType = EnumGType.NetGame,
            GameVersionId = (int)gameVersion,
            GameVersion = version.Name,
            AccessToken = InfoManager.GetGameAccount().Token,
            ServerIp = address.Host,
            ServerPort = address.Port,
            MaxGameMemory = 4096,
            LoadCoreMods = true
        };

        // 启动白端游戏
        var launcherService = new LauncherService(launchRequest);
        await launcherService.LaunchGameAsync();
        ActiveGameAndProxies.Add(launcherService);
    }

    private static void ExEnvironment(EnumGameVersion gameVersion)
    {
        ExEnvironmentByJava(gameVersion);
        ExEnvironmentByNatives(gameVersion);
    }


    // 检测并自动处理natives
    private static void ExEnvironmentByNatives(EnumGameVersion gameVersion)
    {
        // Win 使用盒子资源
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        var version = GameVersionUtil.GetGameVersionFromEnum(gameVersion);
        var mode = PublicProgram.Mode + "." + version + ".natives";
        var num = UpdateTools.CheckUpdate(mode, "Natives").Result;
        // 没有更新的资源
        if (num > 0) return;
        Log.Error("version:{version}", mode);
        Log.Error("该版本可能不支持，如有需要，建议联系开发者。");
    }

    // 检测并自动安装java环境
    private static void ExEnvironmentByJava(EnumGameVersion gameVersion)
    {
        // 检查并安装Java环境
        string javaName;
        string javaPath;
        if (gameVersion >= EnumGameVersion.V_1_16)
        {
            javaName = "jdk17";
            javaPath = PathUtil.Jre17Path;
        }
        else
        {
            javaName = "jre8";
            javaPath = PathUtil.Jre8Path;
        }

        // 不存在
        if (!PathUtil.ExistJava(javaPath))
            UpdateTools.CheckUpdate(PublicProgram.Mode + "." + PublicProgram.Arch + "." + javaName + ".java", "Java")
                .Wait();

        Log.Information("Java Path: {JavaPath}", javaPath);
    }
    
}