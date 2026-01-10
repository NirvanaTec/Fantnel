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
using OpenSDK.Cipher.Nirvana;
using OpenSDK.Cipher.Nirvana.Protocols;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils;
using TokenUtil = WPFLauncherApi.Utils.Cipher.TokenUtil;

namespace Codexus.Game.Launcher.Services.Java;

public sealed class LauncherService : IDisposable
{
    private readonly IProgress<EntityProgressUpdate> _progress;

    private readonly Skip32Cipher _skip32;

    private readonly int _socketPort;

    private AuthLibProtocol _authLibProtocol;

    private bool _disposed;

    private GameRpcService _gameRpcService;

    private EntityModsList _modList;

    public LauncherService(EntityLaunchGame entityLaunchGame)
    {
        Entity = entityLaunchGame;
        _progress = new Progress<EntityProgressUpdate>();
        _skip32 = new Skip32Cipher((from c in "SaintSteve".ToCharArray() select (byte)c).ToArray());
        _socketPort = NetworkUtil.GetAvailablePort(9876);
        Identifier = Guid.NewGuid();
        LastProgress = new EntityProgressUpdate
        {
            Id = Identifier,
            Percent = 0,
            Message = "Initialized"
        };
    }

    public EntityLaunchGame Entity { get; }

    private Guid Identifier { get; }

    private Process GameProcess { get; set; }

    public EntityProgressUpdate LastProgress { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // ReSharper disable once EventNeverSubscribedTo.Global
    public event Action<Guid> Exited;

    public Process GetProcess()
    {
        return GameProcess;
    }

    public Task ShutdownAsync()
    {
        try
        {
            _gameRpcService?.CloseControlConnection();
            if (IsRunning())
            {
                _authLibProtocol?.Dispose();
                _gameRpcService?.CloseControlConnection();
                GameProcess.Kill();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error occurred during shutdown");
        }

        return Task.CompletedTask;
    }

    // 是否运行中
    public bool IsRunning()
    {
        return GameProcess is { HasExited: false };
    }

    public async Task<LauncherService> LaunchGameAsync()
    {
        var progressHandler = CreateProgressHandler();
        try
        {
            await ExecuteLaunchStepsAsync(progressHandler);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch game");
            ReportProgress(progressHandler, 100, "Game launch failed");
            throw;
        }

        return this;
    }

    private async Task ExecuteLaunchStepsAsync(IProgress<EntityProgressUpdate> progressHandler)
    {
        ReportProgress(progressHandler, 5, "Installing game mods");
        var enumVersion = GameVersionConverter.Convert(Entity.GameVersionId);
        _modList = await InstallGameModsAsync(enumVersion);
        ReportProgress(progressHandler, 30, "Preparing Minecraft client");
        await PrepareMinecraftClientAsync(enumVersion);
        ReportProgress(progressHandler, 45, "Setting up runtime");
        var workingDirectory = SetupGameRuntime();
        ReportProgress(progressHandler, 60, "Applying core mods");
        ApplyCoreMods(workingDirectory);
        ReportProgress(progressHandler, 75, "Initializing launcher");
        var (commandService, rpcPort) = InitializeLauncher(enumVersion, workingDirectory);
        ReportProgress(progressHandler, 80, "Starting RPC service");
        LaunchRpcService(enumVersion, rpcPort);
        ReportProgress(progressHandler, 90, "Starting authentication socket service");
        StartAuthenticationService();
        ReportProgress(progressHandler, 95, "Launching game process");
        await StartGameProcessAsync(commandService, progressHandler);
    }

    private SyncCallback<EntityProgressUpdate> CreateProgressHandler()
    {
        var progress = new SyncProgressBarUtil.ProgressBar();
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update =>
        {
            progress.Update(update.Percent, update.Message);
        });
        return new SyncCallback<EntityProgressUpdate>(update =>
        {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport
            {
                Percent = update.Percent,
                Message = update.Message
            });
            _progress.Report(update);
            LastProgress = update;
        });
    }

    private void ReportProgress(IProgress<EntityProgressUpdate> handler, int percent, string message)
    {
        handler.Report(new EntityProgressUpdate
        {
            Id = Identifier,
            Percent = percent,
            Message = message
        });
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
        commandService.Init(dToken: TokenUtil.GenerateEncryptToken(Entity.AccessToken),
            uuid: _skip32.GenerateRoleUuid(Entity.RoleName, Convert.ToUInt32(Entity.UserId)), gameVersion: enumVersion,
            maxMemory: Entity.MaxGameMemory, roleName: Entity.RoleName, serverIp: Entity.ServerIp,
            serverPort: Entity.ServerPort, userId: Entity.UserId, gameId: Entity.GameId, workPath: workingDirectory,
            socketPort: _socketPort, protocolVersion: X19.GameVersion, isFilter: true, rpcPort: availablePort);
        return (commandService, rpcPort: availablePort);
    }

    private void LaunchRpcService(EnumGameVersion gameVersion, int rpcPort)
    {
        var text = Path.Combine(PathUtil.CachePath, "Skins");
        if (!Directory.Exists(text)) Directory.CreateDirectory(text);
        _gameRpcService = new GameRpcService(rpcPort, Entity.ServerIp, Entity.ServerPort.ToString(), Entity.RoleName,
            Entity.UserId, Entity.AccessToken, gameVersion);
        _gameRpcService.Connect(text);
    }

    private void StartAuthenticationService()
    {
        _authLibProtocol = new AuthLibProtocol(IPAddress.Parse("127.0.0.1"), _socketPort,
            JsonSerializer.Serialize(_modList), Entity.GameVersion, Entity.AccessToken);
        _authLibProtocol.Start();
    }

    private async Task StartGameProcessAsync(CommandService commandService,
        IProgress<EntityProgressUpdate> progressHandler)
    {
        var process = commandService.StartGame();
        if (process != null)
            await HandleSuccessfulLaunch(process, progressHandler);
        else
            HandleFailedLaunch(progressHandler);
    }

    private Task HandleSuccessfulLaunch(Process process, IProgress<EntityProgressUpdate> progressHandler)
    {
        GameProcess = process;
        GameProcess.EnableRaisingEvents = true;
        GameProcess.Exited += OnGameProcessExited;
        ReportProgress(progressHandler, 100, "Running");
        SyncProgressBarUtil.ProgressBar.ClearCurrent();
        Console.WriteLine();
        Log.Information(
            "Game launched successfully. Game Version: {GameVersion}, Process ID: {ProcessId}, Role: {Role}",
            Entity.GameVersion, process.Id, Entity.RoleName);
        return Task.CompletedTask;
    }

    private void HandleFailedLaunch(IProgress<EntityProgressUpdate> progressHandler)
    {
        ReportProgress(progressHandler, 100, "Game launch failed");
        SyncProgressBarUtil.ProgressBar.ClearCurrent();
        Log.Error("Game launch failed. Game Version: {GameVersion}, Role: {Role}", Entity.GameVersion, Entity.RoleName);
    }

    private void OnGameProcessExited(object sender, EventArgs e)
    {
        Exited?.Invoke(Identifier);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
            try
            {
                _authLibProtocol?.Dispose();
                _gameRpcService?.CloseControlConnection();
                var gameProcess = GameProcess;
                if (gameProcess is { HasExited: false })
                {
                    GameProcess.Kill();
                    GameProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error occurred during disposal");
            }

        _disposed = true;
    }

    ~LauncherService()
    {
        Dispose(false);
    }
}