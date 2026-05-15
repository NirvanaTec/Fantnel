using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Nirvana.Cipher.Cipher;
using Nirvana.Cipher.Entities;
using Nirvana.Cipher.Entities.Yggdrasil;
using Nirvana.Cipher.Extensions;
using Nirvana.Cipher.Generator;
using Nirvana.WPFLauncher.Http;
using Serilog;

namespace Nirvana.Cipher.Yggdrasil;

public static class StandardYggdrasil {
    private static readonly byte[] ChaChaNonce = "163 NetEase\n"u8.ToArray();

    private static YggdrasilServer[]? _address;

    public static async Task InitializationAsync()
    {
        _address = await RandomAuthServer();
    }

    public static async Task<Result> JoinServerAsync(GameProfile profile, string serverId, bool login = false)
    {
        if (_address == null) {
            throw new Exception("Not StandardYggdrasil Servers Found.");
        }

        using var client = new TcpClient();

        try {
            var random = new Random();
            var server = _address[random.Next(_address.Length)];

            Log.Information("StandardYggdrasil: {0}:{1}", server.Ip, server.Port);
            await client.ConnectAsync(server.Ip, server.Port);

            if (!client.Connected) {
                throw new TimeoutException($"Connecting to server {server.Ip}:{server.Port} timed out");
            }

            var stream = client.GetStream();
            var initiated = await InitializeConnection(stream, profile);

            if (login) {
                return initiated.IsSuccess ? Result.Success() : Result.Clone(initiated);
            }

            return initiated.IsFailure ? Result.Clone(initiated) : await MakeRequest(stream, profile, serverId, initiated.Value!);
        } catch (SocketException ex) {
            return Result.Failure($"Network error: {ex.Message}");
        } catch (Exception ex) {
            return Result.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private static async Task<Result<byte[]>> InitializeConnection(NetworkStream stream, GameProfile profile)
    {
        using var receive = await stream.ReadSteamWithInt16Async();

        if (receive.Length < 272) {
            return Result<byte[]>.Failure("Invalid response length");
        }

        var loginSeed = new byte[16];
        var signContent = new byte[256];
        receive.ReadExactly(loginSeed);
        receive.ReadExactly(signContent);

        var message = YggdrasilGenerator.GenerateInitializeMessage(profile, loginSeed, signContent);
        await stream.WriteAsync(message);

        using var response = await stream.ReadSteamWithInt16Async();

        if (response.Length < 1) {
            return Result<byte[]>.Failure("Empty response");
        }

        var status = (byte)response.ReadByte();

        return status != 0x00 ? Result<byte[]>.Failure($"Initialization failed with status: 0x{status:X2}") : Result<byte[]>.Success(loginSeed);
    }

    private static async Task<Result> MakeRequest(NetworkStream stream, GameProfile profile, string serverId, byte[] loginSeed)
    {
        var token = profile.User.GetAuthToken();

        var packer = new ChaChaPacker(token.CombineWith(loginSeed), ChaChaNonce, true);
        var unpacker = new ChaChaPacker(loginSeed.CombineWith(token), ChaChaNonce, false);
        var message = packer.PackMessage(9, YggdrasilGenerator.GenerateJoinMessage(profile, serverId, loginSeed));
        await stream.WriteAsync(message);

        using var messageStream = await stream.ReadSteamWithInt16Async();
        var packMessage = messageStream.ToArray();
        var (type, unpackMessage) = unpacker.UnpackMessage(packMessage);
        if (type != 9 || unpackMessage[0] != 0x00) {
            return Result.Failure(Convert.ToHexString([unpackMessage[0]]));
        }

        return Result.Success();
    }

    private static async Task<YggdrasilServer[]> RandomAuthServer()
    {
        var servers = await X19Extensions.UpdateNetease.Api<YggdrasilServer[]>("/authserver.list");

        if (servers == null || servers.Length == 0) {
            throw new Exception("Not StandardYggdrasil Servers Found.");
        }

        return servers;
    }
}