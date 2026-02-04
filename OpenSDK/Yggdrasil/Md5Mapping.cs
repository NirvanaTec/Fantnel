namespace OpenSDK.Yggdrasil;

public static class Md5Mapping {
    private static readonly Dictionary<string, Md5Pair> Mapping = new() {
        { "1.7.10", new Md5Pair("A895FE657915D58F55919CEACD30209D", "538D33D5F35EF01736EDA30F94C61DF6") },
        { "1.8.9", new Md5Pair("A895FE657915D58F55919CEACD30209D", "0CF2074AA7D4B543E35A3D6BB57AF861") },
        { "1.16", new Md5Pair("7B101583C3965371B89A3C9115B27526", "B0712F34B0A584D05D9D29FA68759E29") },
        { "1.12.2", new Md5Pair("A895FE657915D58F55919CEACD30209D", "51581ADD89B8AC5A0D8CCDD0E33EE1DE") },
        { "1.18", new Md5Pair("C3BD2115F23F6FE4B2ADCC7FC4DEFFEA", "56677A2BB31E18246FA241FB02E16D0E") },
        { "1.20", new Md5Pair("2A7A476411A1687A56DC6848829C1AE4", "D285CBF97D9BA30D3C445DBF1C342634") },
        { "1.21", new Md5Pair("684528BF492A84489F825F5599B3E1C6", "574033E7E4841D8AC4C14D7FA5E05337") }
    };

    public static Md5Pair GetMd5FromGameVersion(string version)
    {
        return !Mapping.TryGetValue(version, out var pair)
            ? throw new ArgumentException($"不受支持的游戏版本: {version}")
            : pair;
    }

    public class Md5Pair(string bootstrapMd5, string datFileMd5) {
        public string BootstrapMd5 { get; } = bootstrapMd5;
        public string DatFileMd5 { get; } = datFileMd5;
    }
}