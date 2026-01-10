using System.Text.Json.Serialization;
using OpenSDK.Entities.Config;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;

namespace Codexus.Game.Launcher.Entities;

public class EntityLaunchGame
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("game_name")] public string GameName { get; set; }

    [JsonPropertyName("game_id")] public string GameId { get; init; }

    [JsonPropertyName("role_name")] public string RoleName { get; init; } = string.Empty;

    [JsonPropertyName("user_id")] public string UserId { get; init; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("client_type")] public EnumGameClientType ClientType { get; set; }

    [JsonPropertyName("game_type")] public EnumGType GameType { get; init; }

    [JsonPropertyName("game_version_id")] public int GameVersionId { get; init; }

    [JsonPropertyName("game_version")] public string GameVersion { get; init; } = string.Empty;

    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("server_ip")] public string ServerIp { get; init; }

    [JsonPropertyName("server_port")] public int ServerPort { get; init; }

    [JsonPropertyName("max_game_memory")] public int MaxGameMemory { get; init; }

    [JsonPropertyName("load_core_mods")] public bool LoadCoreMods { get; init; }

    /**
     * 清理 相同/过期 的代理
     * @param gameUser 游戏用户
     * @param serverId 服务器ID
     * @param nickname 昵称
     * @return 是否为同一个用户
     */
    public bool Equals(EntityAccount gameAccount, string serverId, string nickname)
    {
        return Equals(gameAccount?.UserId, serverId, nickname) || Equals(gameAccount);
    }

    /**
     * 判断是否为同一个用户, 服务器, 昵称
     * 主要是为了清理相同的代理，避免重复启动
     * @param userId 用户ID
     * @param serverId 服务器ID
     * @param nickname 昵称
     * @return 是否为同一个用户
     */
    private bool Equals(string userId, string serverId, string nickname)
    {
        return userId == UserId && serverId == GameId && nickname == RoleName;
    }

    /**
     * 判断是否为同一个用户, 但是Token不同
     * 主要是为了清理过期的代理
     * @param userId 用户ID
     * @param userToken 用户Token
     * @return 是否为同一个用户
     */
    private bool Equals(EntityAccount gameUser)
    {
        return gameUser?.UserId == UserId && gameUser?.Token != AccessToken;
    }
}