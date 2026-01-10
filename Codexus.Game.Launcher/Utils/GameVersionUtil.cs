using System;
using System.Collections.Generic;
using WPFLauncherApi.Utils;

namespace Codexus.Game.Launcher.Utils;

public static class GameVersionUtil
{
    private static readonly Dictionary<string, EnumGameVersion> GameVersionDictEx = new()
    {
        {
            "",
            EnumGameVersion.NONE
        },
        {
            "1.7.10",
            EnumGameVersion.V_1_7_10
        },
        {
            "1.10.2",
            EnumGameVersion.V_1_10_2
        },
        {
            "1.8",
            EnumGameVersion.V_1_8
        },
        {
            "1.11.2",
            EnumGameVersion.V_1_11_2
        },
        {
            "1.12",
            EnumGameVersion.V_1_12
        },
        {
            "1.12.2",
            EnumGameVersion.V_1_12_2
        },
        {
            "1.8.8",
            EnumGameVersion.V_1_8_8
        },
        {
            "1.8.9",
            EnumGameVersion.V_1_8_9
        },
        {
            "1.9.4",
            EnumGameVersion.V_1_9_4
        },
        {
            "1.6.4",
            EnumGameVersion.V_1_6_4
        },
        {
            "1.7.2",
            EnumGameVersion.V_1_7_2
        },
        {
            "1.18",
            EnumGameVersion.V_1_18
        },
        {
            "1.20",
            EnumGameVersion.V_1_20
        },
        {
            "1.21",
            EnumGameVersion.V_1_21
        }
    };

    public static string GetGameVersionFromEnum(EnumGameVersion gameVersion)
    {
        return (Enum.GetName(gameVersion) ?? string.Empty).Replace("V_", "").Replace("_libs", "").Replace("_", ".")
            .Trim();
    }

    public static EnumGameVersion GetEnumFromGameVersion(string gameVersion)
    {
        return GameVersionDictEx.GetValueOrDefault(gameVersion, EnumGameVersion.NONE);
    }
}