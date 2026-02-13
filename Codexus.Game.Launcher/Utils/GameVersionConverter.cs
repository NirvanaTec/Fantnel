using WPFLauncherApi.Utils;

namespace Codexus.Game.Launcher.Utils;

public static class GameVersionConverter {
    private static readonly Dictionary<string, EnumGameVersion> VersionMap = new() {
        {
            "",
            EnumGameVersion.NONE
        }, {
            "1",
            EnumGameVersion.V_1_7_10
        }, {
            "2",
            EnumGameVersion.V_1_8
        }, {
            "3",
            EnumGameVersion.V_1_9_4
        }, {
            "5",
            EnumGameVersion.V_1_11_2
        }, {
            "6",
            EnumGameVersion.V_1_8_8
        }, {
            "7",
            EnumGameVersion.V_1_10_2
        }, {
            "8",
            EnumGameVersion.V_1_6_4
        }, {
            "9",
            EnumGameVersion.V_1_7_2
        }, {
            "10",
            EnumGameVersion.V_1_12_2
        }, {
            "11",
            EnumGameVersion.NONE
        }, {
            "12",
            EnumGameVersion.V_1_8_9
        }, {
            "13",
            EnumGameVersion.V_CPP
        }, {
            "14",
            EnumGameVersion.V_1_13_2
        }, {
            "15",
            EnumGameVersion.V_1_14_3
        }, {
            "16",
            EnumGameVersion.V_1_15
        }, {
            "17",
            EnumGameVersion.V_1_16
        }, {
            "18",
            EnumGameVersion.V_RTX
        }, {
            "19",
            EnumGameVersion.V_1_18
        }, {
            "23",
            EnumGameVersion.V_1_19_2
        }, {
            "21",
            EnumGameVersion.V_1_20
        }, {
            "22",
            EnumGameVersion.V_1_20_6
        }, {
            "24",
            EnumGameVersion.V_1_21
        }
    };

    public static EnumGameVersion Convert(int versionId)
    {
        return (from version in VersionMap where versionId == (uint)version.Value select version.Value)
            .FirstOrDefault();
    }

    public static int Convert(EnumGameVersion version)
    {
        foreach (var keyValuePair in VersionMap.Where(
                     (Func<KeyValuePair<string, EnumGameVersion>, bool>)(pair => pair.Value == version))) {
            if (string.IsNullOrEmpty(keyValuePair.Key))
                return 0;
            if (int.TryParse(keyValuePair.Key, out var result))
                return result;
        }

        return -1;
    }
}