using System;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Nirvana.Development.Connection;
using Nirvana.DevPlugin;
using Nirvana.DevPlugin.Enums;
using Nirvana.DevPlugin.Extensions;
using Nirvana.DevPlugin.Packet;
using Serilog;

namespace Nirvana.Development.Packet.Login.Client;

public class CPacketEncryptionResponse : BPacket {
    public static readonly RegisterPacket RegisterPacket = new(EnumConnectionState.Login, EnumPacketDirection.ServerBound, 1);

    public byte[]? SecretKeyEncrypted;
    public byte[]? VerifyTokenEncrypted;

    public override void ReadFromBuffer(BGameConnection connection, IByteBuffer buffer)
    {
        if (ProtocolVersion == EnumProtocolVersion.V1076) {
            SecretKeyEncrypted = buffer.ReadByteArrayFromBuffer(buffer.ReadShort());
            VerifyTokenEncrypted = buffer.ReadByteArrayFromBuffer(buffer.ReadShort());
        } else {
            SecretKeyEncrypted = buffer.ReadByteArrayFromBuffer();
            VerifyTokenEncrypted = buffer.ReadByteArrayFromBuffer();
        }
    }

    public override void WriteToBuffer(IByteBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(SecretKeyEncrypted);
        ArgumentNullException.ThrowIfNull(VerifyTokenEncrypted);
        if (ProtocolVersion == EnumProtocolVersion.V1076) {
            buffer.WriteShort(SecretKeyEncrypted.Length);
            buffer.WriteBytes(SecretKeyEncrypted);
            buffer.WriteShort(VerifyTokenEncrypted.Length);
            buffer.WriteBytes(VerifyTokenEncrypted);
        } else {
            buffer.WriteByteArrayToBuffer(SecretKeyEncrypted);
            buffer.WriteByteArrayToBuffer(VerifyTokenEncrypted);
        }
    }

    public override bool HandlePacket(BGameConnection connection)
    {
        Log.Information("Handling Packet Encryption Response.");
        return false;
    }

    public void SendPacket(BGameConnection connection, byte[] secretKey)
    {
        ArgumentNullException.ThrowIfNull(connection.ServerChannel);
        ProtocolVersion = connection.ProtocolVersion;
        PacketId = RegisterPacket.PacketId;
        connection.ServerChannel.Configuration.AutoRead = false;
        connection.ServerChannel.Configuration.SetOption(ChannelOption.AutoRead, false);
        connection.ServerChannel.WriteAndFlushAsync(this).ContinueWith(channel => {
            if (!channel.IsCompleted) {
                return;
            }

            try {
                Log.Information("Successfully Encryption Response");
                GameConnection.EnableEncryption(connection.ServerChannel, secretKey);
                connection.ServerChannel.Configuration.AutoRead = true;
                connection.ServerChannel.Configuration.SetOption(ChannelOption.AutoRead, true);
            } catch (Exception exception) {
                Log.Error(exception, "Failed Encryption Response");
            }
        });
    }
}