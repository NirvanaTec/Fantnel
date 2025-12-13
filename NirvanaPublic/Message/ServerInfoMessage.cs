using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using NirvanaPublic.Entities;
using NirvanaPublic.Utils.ViewLogger;

namespace NirvanaPublic.Message;

public static class ServerInfoMessage
{
    // 详细信息
    public static Task<EntityQueryNetGameDetailItem?> GetServerId2(string id)
    {
        if (PublicProgram.Services == null) throw new Code.ErrorCodeException(Code.ErrorCode.ServicesNotInitialized);
        if (InfoManager.GameUser == null) throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot);
        return InfoManager.GameUser == null
            ? throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot)
            : Task.FromResult(PublicProgram.Services.Wpf
                .QueryNetGameDetailById(InfoManager.GameUser.UserId, InfoManager.GameUser.AccessToken, id).Data);
    }

    // 获取服务器地址
    public static Task<EntityNetGameServerAddress?> GetServerAddress(string id)
    {
        if (PublicProgram.Services == null) throw new Code.ErrorCodeException(Code.ErrorCode.ServicesNotInitialized);
        if (InfoManager.GameUser == null) throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot);
        return InfoManager.GameUser == null
            ? throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot)
            : Task.FromResult(PublicProgram.Services.Wpf
                .GetNetGameServerAddress(InfoManager.GameUser.UserId, InfoManager.GameUser.AccessToken, id).Data);
    }

    /**
     * 获取服务器上的所有游戏角色
     * @param serverId 服务器ID
     * @return 服务器上的所有游戏角色
     */
    public static Task<EntityGameCharacter[]> GetUserName(string serverId)
    {
        if (PublicProgram.Services == null) throw new Code.ErrorCodeException(Code.ErrorCode.ServicesNotInitialized);
        if (InfoManager.GameUser == null) throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot);
        return InfoManager.GameUser == null
            ? throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot)
            : Task.FromResult(PublicProgram.Services.Wpf.QueryNetGameCharacters(InfoManager.GameUser.UserId,
                InfoManager.GameUser.AccessToken, serverId).Data);
    }

    /**
     * 获取服务器上的指定游戏角色
     * @param serverId 服务器ID
     * @param name 游戏角色名称
     * @return 服务器上的指定游戏角色
     */
    public static async Task<EntityGameCharacter> GetUserName(string serverId, string name)
    {
        var games = await GetUserName(serverId);
        if (games == null) throw new Code.ErrorCodeException(Code.ErrorCode.NotFound);
        foreach (var game in games)
            if (game.Name == name)
                return game;
        throw new Code.ErrorCodeException(Code.ErrorCode.NotFoundName);
    }
}