using NirvanaChat.Entities.Packet;

namespace NirvanaChat.Utils;

public static class MinecraftColorCodeConverter {
    // 颜色代码映射表
    private static readonly Dictionary<string, string> ColorMap = new() {
        { "§0", "black" },
        { "§1", "dark_blue" },
        { "§2", "dark_green" },
        { "§3", "dark_aqua" },
        { "§4", "dark_red" },
        { "§5", "dark_purple" },
        { "§6", "gold" },
        { "§7", "gray" },
        { "§8", "dark_gray" },
        { "§9", "blue" },
        { "§a", "green" },
        { "§b", "aqua" },
        { "§c", "red" },
        { "§d", "light_purple" },
        { "§e", "yellow" },
        { "§f", "white" },
        { "§r", "white" }
    };

    public static EntityChatMessage ParseColoredString(string input)
    {
        var parts = ParseColoredStringList(input);
        return new EntityChatMessage {
            Extra = parts,
            Text = string.Empty
        };
    }

    private static List<EntityChatPart> ParseColoredStringList(string input)
    {
        var parts = new List<EntityChatPart>();
        string? currentColor = null; // 当前颜色状态
        var lastSplitIndex = 0;

        for (var i = 0; i < input.Length; i++) {
            if (input[i] == '§' && i + 1 < input.Length) {
                var colorCode = input[i + 1];

                // 检查是否是有效的颜色代码
                var codeStr = $"§{colorCode}";
                if (ColorMap.TryGetValue(codeStr, out var value)) {
                    // 如果遇到新颜色代码且之前已有文本，则先保存之前的文本段
                    if (i > lastSplitIndex) {
                        var textBeforeColor = input.Substring(lastSplitIndex, i - lastSplitIndex);
                        if (!string.IsNullOrEmpty(textBeforeColor)) {
                            if (currentColor != null) {
                                parts.Add(new EntityChatPart {
                                    Color = currentColor,
                                    Text = textBeforeColor
                                });
                            }
                        }
                    }

                    // 更新当前颜色
                    currentColor = value;
                    // 跳过已处理的颜色代码字符 (§ 和 code)
                    i++;
                    lastSplitIndex = i + 1;
                }
            }
        }

        // 循环结束后，处理最后一个没有颜色代码结尾的文本段
        if (lastSplitIndex < input.Length) {
            var finalText = input[lastSplitIndex..];
            if (!string.IsNullOrEmpty(finalText)) {
                if (currentColor != null) {
                    parts.Add(new EntityChatPart {
                        Color = currentColor,
                        Text = finalText
                    });
                }
            }
        }

        return parts;
    }
}