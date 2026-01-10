using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using Codexus.Game.Launcher.Utils;

namespace Codexus.Game.Launcher.Services.Bedrock;

// ReSharper disable once ClassNeverInstantiated.Global
public class ConfigService
{
    public static void GenerateLaunchConfig(string skinPath, string roleName, string entityId, int port)
    {
        var text = Convert.ToHexString(MD5.HashData(File.ReadAllBytes(skinPath)));
        var contents = JsonSerializer.Serialize(new
        {
            room_info = new
            {
                ip = "127.0.0.1",
                port = (uint)port,
                room_name = "Nirvana Server",
                item_ids = new[] { entityId }
            },
            player_info = new
            {
                user_id = 1,
                user_name = roleName,
                urs = "Nirvana Server"
            },
            skin_info = new
            {
                skin = skinPath.Replace(@"\\", "\\"),
                hash = text.ToLower(),
                slim = false,
                skin_iid = "100"
            },
            misc = new
            {
                multiplayer_game_type = 100,
                auth_server_url = ""
            }
        });
        File.WriteAllTextAsync(Path.Combine(PathUtil.CppGamePath, "launch.cppconfig"), contents).GetAwaiter()
            .GetResult();
    }
}