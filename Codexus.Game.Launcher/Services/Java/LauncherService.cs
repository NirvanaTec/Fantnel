using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Codexus.Development.SDK.Utils;
using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Services.Java.RPC;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using NirvanaAPI.Utils;
using OpenSDK.Cipher.Nirvana;
using OpenSDK.Cipher.Nirvana.Protocols;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils;
using TokenUtil = WPFLauncherApi.Utils.Cipher.TokenUtil;

namespace Codexus.Game.Launcher.Services.Java;

public sealed class LauncherService : IDisposable {
    private readonly Skip32Cipher _skip32;

    private readonly int _socketPort;
    private AuthLibProtocol _authLibProtocol;

    private bool _disposed;

    private GameRpcService _gameRpcService;

    private EntityModsList _modList;

    public LauncherService(EntityLaunchGame entityLaunchGame)
    {
        Entity = entityLaunchGame;
        _skip32 = new Skip32Cipher((from c in "SaintSteve".ToCharArray() select (byte)c).ToArray());
        _socketPort = NetworkUtil.GetAvailablePort(9876);
        Identifier = Guid.NewGuid();
    }

    public EntityLaunchGame Entity { get; }

    private Guid Identifier { get; }

    private Process GameProcess { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Process GetProcess()
    {
        return GameProcess;
    }

    public Task ShutdownAsync()
    {
        try {
            _gameRpcService?.CloseControlConnection();
            if (IsRunning()) {
                _authLibProtocol?.Dispose();
                _gameRpcService?.CloseControlConnection();
                GameProcess.Kill();
            }
        } catch (Exception ex) {
            Log.Warning(ex, "Error occurred during shutdown");
        }

        return Task.CompletedTask;
    }

    /**
     * 是否运行中
     * @return 真:正在运行
     */
    public bool IsRunning()
    {
        return GameProcess is { HasExited: false };
    }

    public async Task<LauncherService> LaunchGameAsync()
    {
        try {
            await ExecuteLaunchStepsAsync();
        } catch (Exception ex) {
            Log.Error(ex, "Failed to launch game");
            throw;
        }

        return this;
    }

    private async Task ExecuteLaunchStepsAsync()
    {
        var enumVersion = GameVersionConverter.Convert(Entity.GameVersionId);
        _modList = await InstallGameModsAsync(enumVersion);
        await PrepareMinecraftClientAsync(enumVersion);
        var workingDirectory = SetupGameRuntime();
        ApplyCoreMods(workingDirectory);
        var (commandService, rpcPort) = InitializeLauncher(enumVersion, workingDirectory);
        LaunchRpcService(enumVersion, rpcPort);
        StartAuthenticationService();
        await StartGameProcessAsync(commandService);
    }

    private async Task<EntityModsList> InstallGameModsAsync(EnumGameVersion enumVersion)
    {
        return await InstallerService.InstallGameMods(enumVersion, Entity.GameId,
            Entity.GameType == EnumGType.ServerGame);
    }

    private static async Task PrepareMinecraftClientAsync(EnumGameVersion enumVersion)
    {
        await InstallerService.PrepareMinecraftClient(enumVersion);
    }

    private string SetupGameRuntime()
    {
        var path = InstallerService.PrepareGameRuntime(Entity.GameId, Entity.RoleName, Entity.GameType);
        InstallerService.InstallNativeDll(GameVersionConverter.Convert(Entity.GameVersionId));
        return Path.Combine(path, ".minecraft");
    }

    private void ApplyCoreMods(string workingDirectory)
    {
        var text = Path.Combine(workingDirectory, "mods");
        if (Entity.LoadCoreMods)
            InstallerService.InstallCoreMods(Entity.GameId, text);
        else
            RemoveCoreModFiles(text);
    }

    private static void RemoveCoreModFiles(string modsPath)
    {
        var array = FileUtil.EnumerateFiles(modsPath, "jar");
        foreach (var text in array)
            if (text.Contains("@3"))
                FileUtil.DeleteFileSafe(text);
    }

    private (CommandService commandService, int rpcPort) InitializeLauncher(EnumGameVersion enumVersion,
        string workingDirectory)
    {
        var commandService = new CommandService();
        var availablePort = NetworkUtil.GetAvailablePort(11413);

        commandService.Init(enumVersion, Entity,
            TokenUtil.GenerateEncryptToken(Entity.AccessToken),
            workingDirectory, _skip32.GenerateRoleUuid(Entity.RoleName, Convert.ToUInt32(Entity.UserId)), _socketPort,
            X19.GameVersion, availablePort);

        return (commandService, rpcPort: availablePort);
    }

    private void LaunchRpcService(EnumGameVersion gameVersion, int rpcPort)
    {
        var text = Path.Combine(PathUtil.CachePath, "Skins");
        Directory.CreateDirectory(text);
        _gameRpcService = new GameRpcService(rpcPort, Entity, gameVersion);
        _gameRpcService.Connect(text);
    }

    private void StartAuthenticationService()
    {
        _authLibProtocol = new AuthLibProtocol(IPAddress.Parse("127.0.0.1"), _socketPort,
            JsonSerializer.Serialize(_modList), Entity.GameVersion, Entity.AccessToken);
        _authLibProtocol.Start();
    }

    private async Task StartGameProcessAsync(CommandService commandService)
    {
        var process = commandService.StartGame();
        if (process != null)
            await HandleSuccessfulLaunch(process);
        else
            HandleFailedLaunch();
    }

    private Task HandleSuccessfulLaunch(Process process)
    {
        GameProcess = process;
        GameProcess.EnableRaisingEvents = true;
        GameProcess.Exited += OnGameProcessExited;
        SyncProgressBarUtil.ProgressBar.ClearCurrent();
        Console.WriteLine();
        Log.Information(
            "Game launched successfully. Game Version: {GameVersion}, Process ID: {ProcessId}, Role: {Role}",
            Entity.GameVersion, process.Id, Entity.RoleName);
        return Task.CompletedTask;
    }

    private void HandleFailedLaunch()
    {
        SyncProgressBarUtil.ProgressBar.ClearCurrent();
        Log.Error("Game launch failed. Game Version: {GameVersion}, Role: {Role}", Entity.GameVersion, Entity.RoleName);
    }

    public event Action<Guid> Exited;

    private void OnGameProcessExited(object sender, EventArgs e)
    {
        Exited?.Invoke(Identifier);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
            try {
                _authLibProtocol?.Dispose();
                _gameRpcService?.CloseControlConnection();
                var gameProcess = GameProcess;
                if (gameProcess is { HasExited: false }) {
                    GameProcess.Kill();
                    GameProcess.Dispose();
                }
            } catch (Exception ex) {
                Log.Warning(ex, "Error occurred during disposal");
            }

        _disposed = true;
    }

    ~LauncherService()
    {
        Dispose(false);
    }
}