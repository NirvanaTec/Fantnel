using System.Diagnostics;

namespace Codexus.Game.Launcher.Services.Bedrock;

public static class CommandService
{
	public static Process? StartGame(string exePath, string argumentsPath)
	{
		return Process.Start(new ProcessStartInfo(exePath)
		{
			FileName = exePath,
			Arguments = " config=\"" + argumentsPath + "\"",
			UseShellExecute = false,
			RedirectStandardOutput = false,
			CreateNoWindow = true,
			RedirectStandardError = false
		});
	}
}
