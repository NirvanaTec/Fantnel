namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;

public enum EnumGType
{
    None = -1, // 0xFFFFFFFF
    SingleGame = 1, // 单人游戏
    NetGame = 2, // 网络游戏
    McGame = 7,
    ServerGame = 8, // 租赁服游戏
    LanGame = 9, // 本地联机
    OnlineLobbyGame = 10 // 0x0000000A
}