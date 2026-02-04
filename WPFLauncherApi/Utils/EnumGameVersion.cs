using System.Xml.Serialization;

namespace WPFLauncherApi.Utils;

public enum EnumGameVersion : uint {
    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "")] NONE = 0,

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.6.4")] V_1_6_4 = 1006004, // 0x000F59B4

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.7.2")] V_1_7_2 = 1007002, // 0x000F5D9A

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.7.10")] V_1_7_10 = 1007010, // 0x000F5DA2

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.8")] V_1_8 = 1008000, // 0x000F6180

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.8.8")] V_1_8_8 = 1008008, // 0x000F6188

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.8.9")] V_1_8_9 = 1008009, // 0x000F6189

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.9.4")] V_1_9_4 = 1009004, // 0x000F656C

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.10.2")] V_1_10_2 = 1010002, // 0x000F6952

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.11.2")] V_1_11_2 = 1011002, // 0x000F6D3A

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.12")] V_1_12 = 1012000, // 0x000F7120

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.12.2")] V_1_12_2 = 1012002, // 0x000F7122

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.13.2")] V_1_13_2 = 1013002, // 0x000F750A

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.14.3")] V_1_14_3 = 1014003, // 0x000F78F3

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.15")] V_1_15 = 1015000, // 0x000F7CD8

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.16")] V_1_16 = 1016000, // 0x000F80C0

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.18")] V_1_18 = 1018000, // 0x000F8890

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.19.2")] V_1_19_2 = 1019002, // 0x000F8C7A

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.20")] V_1_20 = 1020000, // 0x000F9060

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.20.6")] V_1_20_6 = 1020006, // 0x000F9066

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "1.21")] V_1_21 = 1021000, // 0x000F9448

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "100.0.0")] V_CPP = 100000000, // 0x05F5E100

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "200.0.0")] V_RTX = 200000000, // 0x0BEBC200

    // ReSharper disable once InconsistentNaming
    [XmlEnum(Name = "300.0.0")] V_X64_CPP = 300000000 // 0x11E1A300
}