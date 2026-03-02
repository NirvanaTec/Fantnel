using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityNirvanaConfig : EntityNirvanaAccount {
    [JsonPropertyName("days")]
    public double Days { get; set; } // 剩余天数

    [JsonPropertyName("hideAccount")]
    public bool HideAccount { get; set; } // 隐藏账号

    [JsonPropertyName("friendly")]
    public bool Friendly { get; set; } // 友好模式

    [JsonPropertyName("chatEnable")]
    public bool ChatEnable { get; set; } // 聊天功能

    [JsonPropertyName("chatTarget")]
    public bool ChatTarget { get; set; } // 玩家标识

    [JsonPropertyName("chatPrefix")]
    public string ChatPrefix { get; set; } = "§6§l涅槃科技 §8|§r "; // 玩家标识

    [JsonPropertyName("gameMemory")]
    public int GameMemory { get; set; } = 4096; // 游戏内存

    [JsonPropertyName("jvmArgs")]
    public string JvmArgs { get; set; } = NirvanaConfig.JvmArgsConst; // 虚拟机参数

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

    public void SetHideAccount(string value)
    {
        HideAccount = value == "true";
    }

    public void SetFriendly(string value)
    {
        Friendly = value == "true";
    }

    public void SetChatEnable(string value)
    {
        ChatEnable = value == "true";
    }

    public void SetChatTarget(string value)
    {
        ChatTarget = value == "true";
    }

    public void SetUseJavaW(string value)
    {
        UseJavaW = value == "true";
    }
}