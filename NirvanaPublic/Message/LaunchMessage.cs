using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Services.Java;
using Codexus.Game.Launcher.Utils;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Protocol;
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

        // 检查并安装Java环境
        UpdateTools.CheckUpdate(PublicProgram.Mode + "." + PublicProgram.Arch + ".java", "Java").Wait();

        // 启动白端游戏
        var launcherService = await new LauncherService(launchRequest).LaunchGameAsync();
        ActiveGameAndProxies.Add(launcherService);
    }

    // 获取已启动白端游戏
    public static List<EntityLaunchGame> GetLauncherService()
    {
        ActiveGameAndProxies.Dispose();
        List<EntityLaunchGame> list = [];
        list.AddRange(ActiveGameAndProxies.ActiveLaunchers.Select(launcher => launcher.Entity));
        return list;
    }
}