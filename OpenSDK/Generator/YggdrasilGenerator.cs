using System.Text;
using OpenSDK.Cipher;
using OpenSDK.Entities.Yggdrasil;
using OpenSDK.Extensions;
using Org.BouncyCastle.Crypto;
using WPFLauncherApi.Protocol;

namespace OpenSDK.Generator;

public class YggdrasilGenerator
{
    private const string PrcCheck = "[]";
    private const int ClientKeyLength = 19;
    private const int CheckSumLength = 32;

    private static readonly byte[] McVersionSalt = [0x01, 0x00, 0x04, 0x80, 0xD2, 0x3E, 0xF7, 0x11, 0x01];

    private static readonly byte[] TcpSalt =
        [0x2F, 0x84, 0xAE, 0xA3, 0x99, 0x21, 0x29, 0x26, 0xDA, 0xBF, 0x95, 0xA3, 0xAB, 0xAF, 0x37, 0xE0];

    private static readonly AsymmetricKeyParameter PublicKey = Rsa.LoadPublicKey(
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4HJFrYdVTeoSvH6qsnJElfXuf7FnxxFQdz3gRCs66LDrZfaoGoWt2e/aGIOv8uGHliBWnZP42Ike9Qf5aiYVtQ4mlj2bXZjifHG35LlS1Bq6yCA6k1WevWcrGWOuLzny3jo8Wbdi0lIFMTT2hN98sF2k4YcvyE9zhqxfRNFGVI5kLyxm9CeTKAXGBU5mw3yQWJ8cPRR4866jpGGOhBWlJdilWt2NES9bid8SbhTT55wqumnVO5J5/DaMyTgKIQngH7NyZQljAhdK5I23dzpGop322n2eQ+mTNLuquwU453o1cbyQobgC6vh5/F1QT2INBR2qYCnRzzJ6hrhE5kIMZwIDAQAB");

    private static readonly AsymmetricKeyParameter PrivateKey = Rsa.LoadPrivateKey(
        "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQDobJGIddxcs08xzTIVFanc4/84J1DbxiW7wLokIrap3txzyyMXj+AcDa8jLopLJkMg9rZLzL50Dwp+hmTTgiIcjkM1DVREsRBltzqjVNyiYPL2VGHLn+/eEivjhLNWUMcWrlAoJ5JJBi22oGPLlKDVJpg33JPI5nPZpwufdB0ecn2V0CAeeGyswQyAaqIjoiYOoP3HipjEYGQHp1RsADf4ozGRgv+2HGiSOyhlv00ixnF0nRUTzVh18ka0N3LdLMpAN/YAPkO8tmoWp0asyU3X+nyZFd29povvrRy4rL1lYmo8jdpfpSL+Yk+9+RybjBAhXRx6uDoBxaa2kwE4fyrrAgMBAAECggEAWT42szruHfoLkofDjyz+R/6TZLBT788pdeoOjwl1McyMwTlihA2Oc7cdZFjeaPSMGgAhBwHarx2HXgWkeUIibuyBCcHQdX+3WBb+wPA4t3CaWdMUqecDZzV6/KVbZu0lRKQxyvlGxhtFOjZjmyu6hZ2IHQrpA97Y5N2rLNKcy69W+QYJZappmBfbVgWM0NRmgmpg6siQ0Cm6Ryil3SBAPBVv+EUPiD9jdXbtVq7VwN4YmwUGScp2Fib1oUnqEAja1hfihVnRFQ246nKXhIc/YVrNmwBrxAVwaaPFRka6XjKkSF0WVbpqQLXhbY1fS8FoXGpVhiF6o4rTQJbpQxhQgQKBgQD7XysfPNt9G63gTrkZvjvEk5LKsRG42MAYEkuzxEal9E/AQv0jJrk0f1WO09hCcYQeaOQXhM9mezNQv5jXEnmepXqM3NTw1Di5yh3uvjSXQjdUt+7haNw+QjggBqZxyQZjtadYairSzfmWe7OwJIkCmdgaJKxm4qMExk9kUgApGwKBgQDstBdJHU/KEBqVpsIlu185vFFuaAvxiHjXHqGwytJMQ/5aVqaphIiQCAxaEogPSzPSm28UHVQiZeFO059EKOpSJscW4pV95Dr5BAbHuYecacqnZKbQqb69//Cfpne9tGYXlmP6QnYfPoc4wYfTfPyU2x3KtDhVxtEDutpSNU1ycQKBgBabWn92s66uvJZ9vfvotetZ8ku0XQmoxK3lh1Vlg40NSdbar3Vn2CQ2h3VO7BYdq2oouMq8sQJgdh7+/DnreXChJUJh4ey+yVM8MDD2fjhURjGiUSOIkLYwsmd+8Z0uHRr+jUxQUAWhbJ7yBRkEUCYhu+OuBKtEGrElPKKjFUydAoGAQLj9pQBe0OGWY1U1wRt67k6P9aB9o42tfSTjEXRkDHaLFiibab7TmI6a0gY/Le9iPDREKzvZxY4WDXfQFNMbP1tbFObf+Yxuk6iGMhaI/jvvLdZXxrajcVCKex0JoNWzFMAKlmOV6PUwBFTmzu1eI1XGz6Z3wPycKmjtSY1JoAECgYBBOfaUDMaG1xLzv+q1jPPs2U4lXPK2BXFE5RaliUGC+LIQREXPishII2LYFW3gtXj5QWfIGq6x0d6ca6Bja2vYRDDe5tlT/2VbZahiHpb2PYL/2WgeoHl7sT9Bb/nsKyo85Sv+doop6huy4+aeTiQHgrGR9JYMVBSIx6P8Tt5phA==");

    public byte[] GenerateJoinMessage(GameProfile profile, string serverId, byte[] loginSeed)
    {
        var time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var hashData = BuildHashData(profile, time, profile.User.UserId, loginSeed);

        using var stream = new MemoryStream();

        stream.WriteLong(long.Parse(profile.GameId));
        stream.WriteString(serverId);
        stream.WriteString(X19.GameVersion);
        stream.WriteString(profile.GameVersion);
        stream.WriteInt(time);
        stream.WriteBytes(hashData);

        var modInfo = profile.GetModInfo();
        stream.WriteShortString(modInfo);
        stream.WriteShortString(PrcCheck);

        stream.WriteShort(0);
        stream.WriteByteLengthString(X19.Channel);

        return stream.ToArray();
    }

    public byte[] GenerateInitializeMessage(GameProfile profile, byte[] loginSeed, byte[] signContent)
    {
        var id = profile.User.GetAuthId();
        var token = profile.User.GetAuthToken();
        var seed = AesNoPadding.Encrypt(loginSeed, token);
        var sign = BuildSign(profile, id, seed).EncodeSha256();

        var client = Rsa.RsaWithPkcs1(PublicKey, signContent, false);

        if (client.Length < ClientKeyLength + CheckSumLength)
            throw new ArgumentException("Invalid sign content length");

        var clientKey = client.AsSpan(0, ClientKeyLength).ToArray();
        var checkSum = client.AsSpan(ClientKeyLength, CheckSumLength).ToArray();

        if (!checkSum.SequenceEqual(loginSeed.EncodeSha256()))
            throw new InvalidOperationException("CheckSum validation failed");

        var signData = Rsa.RsaWithPkcs1(PrivateKey, clientKey.CombineWith(sign), true);

        using var stream = new MemoryStream();
        stream.WriteInt(id);
        stream.WriteBytes(seed);
        stream.WriteShortString(X19.GameVersion, false);
        stream.WriteByteLengthString(X19.Channel);
        stream.WriteBytes(TcpSalt);
        stream.WriteShortBytes(signData);
        stream.WriteByteLengthString(profile.GameVersion);
        stream.WriteBytes(McVersionSalt);

        using var message = new MemoryStream();
        message.WriteShort((int)stream.Length);
        message.Write(stream.ToArray());

        return message.ToArray();
    }

    private byte[] BuildSign(GameProfile profile, int id, byte[] seed)
    {
        using var stream = new MemoryStream();
        var encoding = Encoding.UTF8;

        stream.WriteInt(id);
        stream.WriteBytes(seed);
        stream.WriteBytes(encoding.GetBytes(X19.GameVersion));
        stream.WriteBytes(encoding.GetBytes(X19.Channel));
        stream.WriteBytes(encoding.GetBytes(X19.GetCrcSalt()));
        stream.WriteBytes(encoding.GetBytes(profile.GameVersion));
        stream.WriteBytes(McVersionSalt);

        return stream.ToArray();
    }

    private static byte[] BuildHashData(GameProfile profile, int time, int id, byte[] loginSeed)
    {
        var joinMessage =
            $"{X19.GameVersion}{profile.GameVersion}{time}{X19.GetCrcSalt()}{profile.GetModInfo()}{profile.BootstrapMd5}{profile.DatFileMd5}{PrcCheck}";

        return Encoding.UTF8.GetBytes(joinMessage)
            .CombineWith(id.ToByteArray())
            .CombineWith(loginSeed)
            .EncodeSha256();
    }
}