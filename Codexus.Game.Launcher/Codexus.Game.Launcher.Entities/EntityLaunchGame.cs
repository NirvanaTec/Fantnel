using System.Text.Json.Serialization;
using Codexus.Cipher.Entities;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Texture;

namespace Codexus.Game.Launcher.Entities;

public class EntityLaunchGame
{
	[JsonPropertyName("game_name")]
	public string GameName { get; set; }

	[JsonPropertyName("game_id")]
	public string GameId { get; set; }

	[JsonPropertyName("role_name")]
	public string RoleName { get; set; } = string.Empty;

	[JsonPropertyName("user_id")]
	public string UserId { get; set; }

	[JsonPropertyName("client_type")]
	public EnumGameClientType ClientType { get; set; }

	[JsonPropertyName("game_type")]
	public EnumGType GameType { get; set; }

	[JsonPropertyName("game_version_id")]
	public int GameVersionId { get; set; }

	[JsonPropertyName("game_version")]
	public string GameVersion { get; set; } = string.Empty;

	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = string.Empty;

	[JsonPropertyName("server_ip")]
	public string ServerIp { get; set; }

	[JsonPropertyName("server_port")]
	public int ServerPort { get; set; }

	[JsonPropertyName("max_game_memory")]
	public int MaxGameMemory { get; set; }

	[JsonPropertyName("load_core_mods")]
	public bool LoadCoreMods { get; set; }
}
