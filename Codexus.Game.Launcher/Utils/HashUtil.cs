namespace Codexus.Game.Launcher.Utils;

public static class HashUtil
{
    public static string GenerateGameRuntimeId(string gameId, string roleName)
    {
        return gameId + "-" + roleName;
    }
}