using System.Text.Json.Serialization;
using Codexus.Cipher.Entities;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Texture;

namespace Codexus.Game.Launcher.Entities;

public class EntityLaunchPeGame
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

	[JsonPropertyName("launch_type")]
	public EnumLaunchType LaunchType { get; set; }

	[JsonPropertyName("launch_path")]
	public string? LaunchPath { get; set; }

	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = string.Empty;

	[JsonPropertyName("server_ip")]
	public string ServerIp { get; set; }

	[JsonPropertyName("server_port")]
	public int ServerPort { get; set; }

	[JsonPropertyName("skin_path")]
	public string SkinPath { get; set; } = string.Empty;
}
