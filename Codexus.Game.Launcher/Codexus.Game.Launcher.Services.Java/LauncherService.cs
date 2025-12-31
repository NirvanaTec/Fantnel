using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Codexus.Cipher.Connection.Protocols;
using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Mods;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Texture;
using Codexus.Cipher.Protocol;
using Codexus.Cipher.Skip32;
using Codexus.Cipher.Utils;
using Codexus.Cipher.Utils.Cipher;
using Codexus.Development.SDK.Utils;
using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Services.Java.RPC;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using Serilog;

namespace Codexus.Game.Launcher.Services.Java;

public sealed class LauncherService : IDisposable
{
	private const string Skip32Key = "SaintSteve";

	private const int DefaultSocketPort = 9876;

	private const int DefaultRpcPort = 11413;

	private const string JavaExeName = "javaw.exe";

	private const string MinecraftDirectory = ".minecraft";

	private const string ModsDirectory = "mods";

	private const string SkinsDirectory = "Skins";

	private const string CoreModPattern = "@3";

	private const string JarExtension = "jar";

	private readonly IProgress<EntityProgressUpdate> _progress;

	private readonly string _protocolVersion;

	private readonly Skip32Cipher _skip32;

	private readonly int _socketPort;

	private readonly string _userToken;

	private readonly WPFLauncher _wpf;

	private AuthLibProtocol? _authLibProtocol;

	private GameRpcService? _gameRpcService;

	private EntityModsList? _modList;

	private bool _disposed;

	public EntityLaunchGame Entity { get; }

	public Guid Identifier { get; }

	private Process? GameProcess { get; set; }

	public EntityProgressUpdate LastProgress { get; private set; }

	public event Action<Guid>? Exited;

	private LauncherService(EntityLaunchGame entityLaunchGame, string userToken, WPFLauncher wpfLauncher, string protocolVersion, IProgress<EntityProgressUpdate> progress)
	{
		Entity = entityLaunchGame ?? throw new ArgumentNullException("entityLaunchGame");
		_userToken = userToken ?? throw new ArgumentNullException("userToken");
		_wpf = wpfLauncher ?? throw new ArgumentNullException("wpfLauncher");
		_protocolVersion = protocolVersion ?? throw new ArgumentNullException("protocolVersion");
		_progress = progress ?? throw new ArgumentNullException("progress");
		_skip32 = new Skip32Cipher((from c in "SaintSteve".ToCharArray()
			select (byte)c).ToArray());
		_socketPort = NetworkUtil.GetAvailablePort(9876);
		Identifier = Guid.NewGuid();
		LastProgress = new EntityProgressUpdate
		{
			Id = Identifier,
			Percent = 0,
			Message = "Initialized"
		};
	}

	public static LauncherService CreateLauncher(EntityLaunchGame entityLaunchGame, string userToken, WPFLauncher wpfLauncher, string protocolVersion, IProgress<EntityProgressUpdate> progress)
	{
		LauncherService launcherService = new LauncherService(entityLaunchGame, userToken, wpfLauncher, protocolVersion, progress);
		Task.Run((Func<Task?>)launcherService.LaunchGameAsync);
		return launcherService;
	}

	public Process? GetProcess()
	{
		return GameProcess;
	}

	public async Task ShutdownAsync()
	{
		try
		{
			_gameRpcService?.CloseControlConnection();
			Process gameProcess = GameProcess;
			if (gameProcess != null && !gameProcess.HasExited)
			{
				GameProcess.Kill();
				await GameProcess.WaitForExitAsync();
			}
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Error occurred during shutdown");
		}
	}

	private async Task LaunchGameAsync()
	{
		IProgress<EntityProgressUpdate> progressHandler = CreateProgressHandler();
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
	}

	private async Task ExecuteLaunchStepsAsync(IProgress<EntityProgressUpdate> progressHandler)
	{
		ReportProgress(progressHandler, 5, "Installing game mods");
		EnumGameVersion enumVersion = GameVersionConverter.Convert(Entity.GameVersionId);
		_modList = await InstallGameModsAsync(enumVersion);
		ReportProgress(progressHandler, 15, "Preparing Java runtime");
		await PrepareJavaRuntimeAsync(enumVersion);
		ReportProgress(progressHandler, 30, "Preparing Minecraft client");
		await PrepareMinecraftClientAsync(enumVersion);
		ReportProgress(progressHandler, 45, "Setting up runtime");
		string workingDirectory = SetupGameRuntime();
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

	private IProgress<EntityProgressUpdate> CreateProgressHandler()
	{
		SyncProgressBarUtil.ProgressBar progress = new SyncProgressBarUtil.ProgressBar(100);
		SyncCallback<SyncProgressBarUtil.ProgressReport> uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(delegate(SyncProgressBarUtil.ProgressReport update)
		{
			progress.Update(update.Percent, update.Message);
		});
		return new SyncCallback<EntityProgressUpdate>(delegate(EntityProgressUpdate update)
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

	private async Task<EntityModsList?> InstallGameModsAsync(EnumGameVersion enumVersion)
	{
		return await InstallerService.InstallGameMods(Entity.UserId, _userToken, enumVersion, _wpf, Entity.GameId, Entity.GameType == EnumGType.ServerGame);
	}

	private static async Task PrepareJavaRuntimeAsync(EnumGameVersion enumVersion)
	{
		string path = ((enumVersion > EnumGameVersion.V_1_16) ? "jdk17" : "jre8");
		if (!File.Exists(Path.Combine(Path.Combine(PathUtil.JavaPath, path), "bin", "javaw.exe")))
		{
			await JreService.PrepareJavaRuntime();
		}
	}

	private async Task PrepareMinecraftClientAsync(EnumGameVersion enumVersion)
	{
		await InstallerService.PrepareMinecraftClient(Entity.UserId, _userToken, _wpf, enumVersion);
	}

	private string SetupGameRuntime()
	{
		string path = InstallerService.PrepareGameRuntime(Entity.UserId, Entity.GameId, Entity.RoleName, Entity.GameType);
		InstallerService.InstallNativeDll(GameVersionConverter.Convert(Entity.GameVersionId));
		return Path.Combine(path, ".minecraft");
	}

	private void ApplyCoreMods(string workingDirectory)
	{
		string text = Path.Combine(workingDirectory, "mods");
		if (Entity.LoadCoreMods)
		{
			InstallerService.InstallCoreMods(Entity.GameId, text);
		}
		else
		{
			RemoveCoreModFiles(text);
		}
	}

	private static void RemoveCoreModFiles(string modsPath)
	{
		string[] array = FileUtil.EnumerateFiles(modsPath, "jar");
		foreach (string text in array)
		{
			if (text.Contains("@3"))
			{
				FileUtil.DeleteFileSafe(text);
			}
		}
	}

	private (CommandService commandService, int rpcPort) InitializeLauncher(EnumGameVersion enumVersion, string workingDirectory)
	{
		CommandService commandService = new CommandService();
		int availablePort = NetworkUtil.GetAvailablePort(11413);
		commandService.Init(dToken: TokenUtil.GenerateEncryptToken(_userToken), uuid: _skip32.GenerateRoleUuid(Entity.RoleName, Convert.ToUInt32(Entity.UserId)), gameVersion: enumVersion, maxMemory: Entity.MaxGameMemory, roleName: Entity.RoleName, serverIp: Entity.ServerIp, serverPort: Entity.ServerPort, userId: Entity.UserId, gameId: Entity.GameId, workPath: workingDirectory, socketPort: _socketPort, protocolVersion: _protocolVersion, isFilter: true, rpcPort: availablePort);
		return (commandService: commandService, rpcPort: availablePort);
	}

	private void LaunchRpcService(EnumGameVersion gameVersion, int rpcPort)
	{
		string text = Path.Combine(PathUtil.CachePath, "Skins");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		_gameRpcService = new GameRpcService(rpcPort, Entity.ServerIp, Entity.ServerPort.ToString(), Entity.RoleName, Entity.UserId, _userToken, gameVersion);
		_gameRpcService.Connect(text, _wpf.GetSkinListInGame, _wpf.GetNetGameComponentDownloadList);
	}

	private void StartAuthenticationService()
	{
		_authLibProtocol = new AuthLibProtocol(IPAddress.Parse("127.0.0.1"), _socketPort, JsonSerializer.Serialize(_modList), Entity.GameVersion, Entity.AccessToken);
		_authLibProtocol.Start();
	}

	private async Task StartGameProcessAsync(CommandService commandService, IProgress<EntityProgressUpdate> progressHandler)
	{
		Process process = commandService.StartGame();
		if (process != null)
		{
			await HandleSuccessfulLaunch(process, progressHandler);
		}
		else
		{
			HandleFailedLaunch(progressHandler);
		}
	}

	private Task HandleSuccessfulLaunch(Process process, IProgress<EntityProgressUpdate> progressHandler)
	{
		GameProcess = process;
		GameProcess.EnableRaisingEvents = true;
		GameProcess.Exited += OnGameProcessExited;
		ReportProgress(progressHandler, 100, "Running");
		SyncProgressBarUtil.ProgressBar.ClearCurrent();
		Console.WriteLine();
		Log.Information<string, int, string>("Game launched successfully. Game Version: {GameVersion}, Process ID: {ProcessId}, Role: {Role}", Entity.GameVersion, process.Id, Entity.RoleName);
		MemoryOptimizer.GetInstance();
		return Task.CompletedTask;
	}

	private void HandleFailedLaunch(IProgress<EntityProgressUpdate> progressHandler)
	{
		ReportProgress(progressHandler, 100, "Game launch failed");
		SyncProgressBarUtil.ProgressBar.ClearCurrent();
		Log.Error<string, string>("Game launch failed. Game Version: {GameVersion}, Role: {Role}", Entity.GameVersion, Entity.RoleName);
	}

	private void OnGameProcessExited(object? sender, EventArgs e)
	{
		this.Exited?.Invoke(Identifier);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		if (disposing)
		{
			try
			{
				_authLibProtocol?.Dispose();
				_gameRpcService?.CloseControlConnection();
				Process gameProcess = GameProcess;
				if (gameProcess != null && !gameProcess.HasExited)
				{
					GameProcess.Kill();
					GameProcess.Dispose();
				}
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Error occurred during disposal");
			}
		}
		_disposed = true;
	}

	~LauncherService()
	{
		Dispose(disposing: false);
	}
}
