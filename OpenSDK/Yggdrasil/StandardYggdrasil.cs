using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using OpenSDK.Cipher;
using OpenSDK.Entities;
using OpenSDK.Entities.Yggdrasil;
using OpenSDK.Extensions;
using OpenSDK.Generator;
using WPFLauncherApi.Http;

namespace OpenSDK.Yggdrasil;

public partial class StandardYggdrasil(string address, int port)
{
    private static readonly byte[] ChaChaNonce = "163 NetEase\n"u8.ToArray();
    private readonly YggdrasilGenerator _generator = new();

    private StandardYggdrasil(string address)
        : this(ParseAddress(address).address, ParseAddress(address).port)
    {
    }

    public StandardYggdrasil()
        : this(RandomAuthServer())
    {
    }

    private string Address { get; } = address;
    private int Port { get; } = port;

    public async Task<Result> JoinServerAsync(GameProfile profile, string serverId, bool login = false)
    {
        using var client = new TcpClient();

        try
        {
            var addresses = await ResolveAddressAsync(Address);
            await client.ConnectAsync(addresses[0], Port);

            if (!client.Connected)
                throw new TimeoutException($"Connecting to server {Address}:{Port} timed out");

            var stream = client.GetStream();
            var initiated = await InitializeConnection(stream, profile);

            if (login)
                return initiated.IsSuccess ? Result.Success() : Result.Clone(initiated);

            return initiated.IsFailure
                ? Result.Clone(initiated)
                : await MakeRequest(stream, profile, serverId, initiated.Value!);
        }
        catch (SocketException ex)
        {
            return Result.Failure($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<Result<byte[]>> InitializeConnection(NetworkStream stream, GameProfile profile)
    {
        using var receive = await stream.ReadSteamWithInt16Async();

        if (receive.Length < 272)
            return Result<byte[]>.Failure("Invalid response length");

        var loginSeed = new byte[16];
        var signContent = new byte[256];
        receive.ReadExactly(loginSeed);
        receive.ReadExactly(signContent);

        var message = _generator.GenerateInitializeMessage(profile, loginSeed, signContent);
        await stream.WriteAsync(message);

        using var response = await stream.ReadSteamWithInt16Async();

        if (response.Length < 1)
            return Result<byte[]>.Failure("Empty response");

        var status = (byte)response.ReadByte();

        return status != 0x00
            ? Result<byte[]>.Failure($"Initialization failed with status: 0x{status:X2}")
            : Result<byte[]>.Success(loginSeed);
    }

    private async Task<Result> MakeRequest(NetworkStream stream, GameProfile profile, string serverId, byte[] loginSeed)
    {
        var token = profile.User.GetAuthToken();

        var packer = new ChaChaPacker(token.CombineWith(loginSeed), ChaChaNonce, true);
        var unpacker = new ChaChaPacker(loginSeed.CombineWith(token), ChaChaNonce, false);
        var message = packer.PackMessage(9, _generator.GenerateJoinMessage(profile, serverId, loginSeed));
        await stream.WriteAsync(message);

        using var messageStream = await stream.ReadSteamWithInt16Async();
        var packMessage = messageStream.ToArray();
        var (type, unpackMessage) = unpacker.UnpackMessage(packMessage);
        if (type != 9 || unpackMessage[0] != 0x00) return Result.Failure(Convert.ToHexString([unpackMessage[0]]));

        return Result.Success();
    }

    private static (string address, int port) ParseAddress(string baseAddress)
    {
        var match = AddressRegex().Match(baseAddress);
        if (!match.Success)
            throw new FormatException($"Invalid address format: {baseAddress}");

        var address = match.Groups["address"].Value;
        var port = match.Groups["port"].Success
            ? int.Parse(match.Groups["port"].Value)
            : throw new FormatException($"Invalid port format: {baseAddress}");

        return (address, port);
    }

    private static async Task<IPAddress[]> ResolveAddressAsync(string address)
    {
        if (IPAddress.TryParse(address, out var ipAddress))
            return [ipAddress];

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(address);
            return addresses.Length == 0
                ? throw new InvalidOperationException($"Unable to resolve host: {address}")
                : addresses;
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException($"DNS resolution failed for {address}", ex);
        }
    }

    private static string RandomAuthServer()
    {
        var http = new HttpWrapper();
        var servers = http.GetAsync<YggdrasilServer[]>("https://x19.update.netease.com/authserver.list").GetAwaiter()
            .GetResult();

        if (servers == null || servers.Length == 0)
            throw new Exception("No servers found.");

        var random = new Random();
        var selected = servers[random.Next(servers.Length)];
        return selected.Ip + ":" + selected.Port;
    }

    [GeneratedRegex(@"^(?<address>[^:]+):(?<port>\d+)$")]
    private static partial Regex AddressRegex();
}