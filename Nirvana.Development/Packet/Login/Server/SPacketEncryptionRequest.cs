using System;
using System.IO;
using System.Text;
using DotNetty.Buffers;
using Nirvana.Development.Manager;
using Nirvana.Development.Packet.Login.Client;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Events.Event;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace Nirvana.Development.Packet.Login.Server;

public class SPacketEncryptionRequest : DPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ClientBound, 1);

    private byte[]? _publicKey;
    private string? _serverId;
    private byte[]? _verifyToken;

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        if (ProtocolVersion == EnumProtocolVersion.V1076) {
            _serverId = buffer.ReadStringFromBuffer(20);
            _publicKey = buffer.ReadByteArrayFromBuffer(buffer.ReadShort());
            _verifyToken = buffer.ReadByteArrayFromBuffer(buffer.ReadShort());
            return;
        }

        _serverId = buffer.ReadStringFromBuffer(20);
        _publicKey = buffer.ReadByteArrayFromBuffer();
        _verifyToken = buffer.ReadByteArrayFromBuffer();
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        ArgumentNullException.ThrowIfNull(_serverId);
        ArgumentNullException.ThrowIfNull(_verifyToken);

        var cipherKeyGenerator = new CipherKeyGenerator();
        cipherKeyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 128));
        var instance = SubjectPublicKeyInfo.GetInstance(_publicKey);
        var secretKey = cipherKeyGenerator.GenerateKey();
        using var memoryStream = new MemoryStream(20);
        memoryStream.Write(Encoding.GetEncoding("ISO-8859-1").GetBytes(_serverId));
        memoryStream.Write(secretKey);
        memoryStream.Write(_publicKey);
        memoryStream.Position = 0L;
        var text = memoryStream.ToSha1();

        if (EventManager.TriggerEvent<IEventEncryptionRequest>(request => request.OnEncryptionRequest(_serverId), ProtocolVersion) == null) {
            connection.Config.OnJoinServer?.Invoke(connection.Config, text);
        }

        var pkcs1Encoding = new Pkcs1Encoding(new RsaEngine());
        pkcs1Encoding.Init(true, PublicKeyFactory.CreateKey(instance));

        var message = new CPacketEncryptionResponse {
            SecretKeyEncrypted = pkcs1Encoding.ProcessBlock(secretKey, 0, secretKey.Length),
            VerifyTokenEncrypted = pkcs1Encoding.ProcessBlock(_verifyToken, 0, _verifyToken.Length)
        };
        message.SendPacket(connection, secretKey);
        return true;
    }
}