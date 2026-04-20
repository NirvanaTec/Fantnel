using NirvanaAPI.Utils;

namespace Nirvana.Game.Launcher.Utils;

public class GameArgumentsUtil {
    
    private static readonly List<string> StartList = [
        " --", // 0--cp="123" | 1--cp=123 | 2--cp "123" | 3--cp 123 | 4--Xmx123
        " -D", // 5-Dcp="123" | 6-Dcp=123 | 7-Dcp "123" | 8-Dcp 123 | 9-DXmx123
        " -" // 10-cp="123" | 11-cp=123 | 12-cp "123" | 13-cp 123 | 14-Xmx123
    ];

    private static readonly List<string> BetweenList = [
        "=\"",
        "=",
        " \"",
        " ",
        ""
    ];

    private static readonly List<string> EndList = [
        "\"", // > "\" "
        "", // > " "
        "\"", // > "\" "
        "", // > " "
        "" // > ""
    ];

    public static string AddArguments(string text, string tex1)
    {
        string value;
        
        // 最后1个字符是空格
        if (text.EndsWith(' ')) {
            value = text + tex1;
        } else {
            value = text + " " + tex1;
        }

        // 删除最后1个空格
        return value.TrimEnd(' ');
    }

    private static string GenArguments(string name, string value, CommandMode mode)
    {
        var commandMode = GetCommandMode(mode);
        return commandMode.Item1 + name + commandMode.Item2 + value + commandMode.Item3;
    }

    public static string UpdateArguments(string name, string value, string text, params CommandMode[] mode)
    {
        var arguments = GetArguments(name, text, true, mode);
        var genArguments = string.IsNullOrEmpty(value) ? string.Empty : GenArguments(name, value, mode[0]);
        // "  -a 1 " > " -a 1"
        return string.IsNullOrEmpty(arguments) ? AddArguments(text, genArguments) : text.Replace(arguments, genArguments);
    }

    public static string DeleteArguments(string name, string text, params CommandMode[] mode)
    {
        return UpdateArguments(name, string.Empty, text, mode);
    }

    /**
     * 获取参数值
     * @param name 参数名 cp
     * @param text 文本
     * @param complete 是否返回完整参数 -cp 123
     * @return 参数值
     */
    public static string GetArguments(string name, string text, bool complete = false, params CommandMode[] mode)
    {
        return complete ? GetArguments1(name, text, mode).Item1 : GetArguments1(name, text, mode).Item2;
    }

    /**
     * 获取参数值
     * @param name 参数名 cp
     * @param text 文本
     * @return 完整参数 [-cp 123], 参数值 [123]
     */
    public static (string, string) GetArguments1(string name, string text, params CommandMode[] mode)
    {
        foreach (var modeBase in mode) {
            var commandMode = GetCommandMode(modeBase);
            var context = commandMode.Item1 + name + commandMode.Item2;
            if (!text.Contains(context)) {
                continue;
            }
            var value = Tools.GetBetweenStrings(text,  context, commandMode.Item3 + " ");
            return (context + value + commandMode.Item3, value);
        }
        return (string.Empty, string.Empty);
    }

    private static (string, string, string) GetCommandMode(CommandMode mode)
    {
        var index = 0;
        foreach (var start in StartList) {
            for (var i = 0; i < BetweenList.Count; i++) {
                if (index++ == (int)mode) {
                    return (start, BetweenList[i], EndList[i]);
                }
            }
        }
        throw new NotImplementedException();
    }
    
}