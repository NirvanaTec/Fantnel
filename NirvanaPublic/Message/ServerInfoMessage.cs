using System.Text.Json.Nodes;
using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;
using Serilog;
using EntityNetGameServerAddress = Codexus.Cipher.Entities.WPFLauncher.NetGame.EntityNetGameServerAddress;

namespace NirvanaPublic.Message;

public static class ServerInfoMessage
{
    // 详细信息
    public static Task<EntityQueryNetGameDetailItem?> GetServerId2(string id)
    {
        return Task.FromResult(InitProgram.GetServices().Wpf
            .QueryNetGameDetailById(InfoManager.GetGameUser().UserId, InfoManager.GetGameUser().AccessToken, id).Data);
    }

    // 获取服务器地址
    public static Task<EntityNetGameServerAddress?> GetServerAddress(string id)
    {
        return Task.FromResult(InitProgram.GetServices().Wpf
            .GetNetGameServerAddress(InfoManager.GetGameUser().UserId, InfoManager.GetGameUser().AccessToken, id).Data);
    }

    /**
     * 获取服务器上的所有游戏角色
     * @param serverId 服务器ID
     * @return 服务器上的所有游戏角色
     */
    public static Task<EntityGameCharacter[]> GetUserName(string serverId)
    {
        return Task.FromResult(InitProgram.GetServices().Wpf.QueryNetGameCharacters(InfoManager.GetGameUser().UserId,
            InfoManager.GetGameUser().AccessToken, serverId).Data);
    }

    /**
     * 获取服务器上的指定游戏角色
     * @param serverId 服务器ID
     * @param name 游戏角色名称
     * @return 服务器上的指定游戏角色
     */
    public static async Task<EntityGameCharacter> GetUserName(string serverId, string name)
    {
        for (var i = 0; i < 3; i++)
        {
            try
            {
                var games = await GetUserName(serverId);
                if (games == null) throw new Code.ErrorCodeException(Code.ErrorCode.NotFound);
                foreach (var game in games)
                    if (game.Name == name)
                        return game;
            }
            catch (Exception e)
            {
                Log.Error("获取游戏角色 {0} 失败 {1}", name, e.Message);
            }

            Thread.Sleep(800);
        }

        throw new Code.ErrorCodeException(Code.ErrorCode.NotFoundName);
    }

    /**
     * 创建游戏角色
     * @param serverId 服务器ID
     * @param name 游戏角色名称
     * @return 创建的游戏角色
     */
    public static void CreateCharacter(string serverId, string name)
    {
        // 检查登录状态
        var createCharacter = X19Extensions.Api<EntityCreateCharacter, JsonObject>("/game-character",
            new EntityCreateCharacter
            {
                GameId = serverId,
                UserId = InfoManager.GetGameUser().UserId,
                Name = name
            }).Result;
        // 检查创建结果
        if (createCharacter == null) throw new Code.ErrorCodeException(Code.ErrorCode.Failure);
        var code = createCharacter["code"];
        if (code == null || code.ToString() != "0")
        {
            // 创建失败
            var error = new Code.ErrorCodeException(Code.ErrorCode.Failure, createCharacter);
            // 创建失败，同步错误信息
            var msg = createCharacter["message"];
            if (msg != null) error.Entity.Msg = msg.ToString();
            throw error;
        }

        // 确保不会因为缓存导致获取失败
        GetUserName(serverId, name).Wait();
    }
}