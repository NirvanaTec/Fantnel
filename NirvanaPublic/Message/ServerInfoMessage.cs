using System.Text.Json.Nodes;
using NirvanaPublic.Manager;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Http;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Message;

public static class ServerInfoMessage
{
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
                var games = await WPFLauncher.QueryNetGameCharactersAsync(serverId);
                if (games == null) throw new ErrorCodeException(ErrorCode.NotFound);
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

        throw new ErrorCodeException(ErrorCode.NotFoundName);
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
        var createCharacter = X19Extensions.Gateway.Api<EntityGameCharacter, JsonObject>("/game-character",
            new EntityGameCharacter
            {
                GameId = serverId,
                UserId = InfoManager.GetGameAccount().GetUserId(),
                Name = name
            }).Result;
        // 检查创建结果
        if (createCharacter == null) throw new ErrorCodeException(ErrorCode.Failure);
        var code = createCharacter["code"];
        if (code == null || code.ToString() != "0")
        {
            // 创建失败
            var error = new ErrorCodeException(ErrorCode.Failure, createCharacter);
            // 创建失败，同步错误信息
            var msg = createCharacter["message"];
            if (msg != null) error.Entity.Msg = msg.ToString();
            throw error;
        }

        // 确保不会因为缓存导致获取失败
        GetUserName(serverId, name).Wait();
    }
}