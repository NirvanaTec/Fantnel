using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityNirvanaConfig : EntityNirvanaAccount {
    [JsonPropertyName("days")]
    public double Days { get; set; } // 剩余天数

    [JsonPropertyName("hideAccount")]
    public bool HideAccount { get; set; } // 隐藏账号

    [JsonPropertyName("chatEnable")]
    public bool ChatEnable { get; set; } // 聊天功能

    [JsonPropertyName("gameMemory")]
    public int GameMemory { get; set; } = 4096; // 游戏内存

    [JsonPropertyName("jvmArgs")]
    public string JvmArgs { get; set; } = "-XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Djdk.lang.Process.allowAmbiguousCommands=true -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true"; // 虚拟机参数

    [JsonPropertyName("gameArgs")]
    public string GameArgs { get; set; } = string.Empty; // 游戏参数

    [JsonPropertyName("autoLoginGame")]
    public bool AutoLoginGame { get; set; } = true; // 自动登录游戏

    [JsonPropertyName("autoLoginGame163Email")]
    public bool AutoLoginGame163Email { get; set; } = true; // 自动登录游戏163Email

    [JsonPropertyName("autoLoginGameCookie")]
    public bool AutoLoginGameCookie { get; set; } = true; // 自动登录游戏Cookie

    [JsonPropertyName("useJavaW")]
    public bool UseJavaW { get; set; } = true; // 使用 javaW.exe

    [JsonPropertyName("autoUpdatePlugin")]
    public bool AutoUpdatePlugin { get; set; } = true; // 自动更新插件
    
}