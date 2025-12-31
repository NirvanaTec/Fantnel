using System;
using System.Diagnostics;
using System.IO;
using Codexus.Game.Launcher.Utils;

namespace Codexus.Game.Launcher.Services.Java;

public static class RepairService
{
	private static Action? _killGameAction;

	public static void RegisterKillGameAction(Action action)
	{
		_killGameAction = action;
	}

	public static void ClearClientResources()
	{
		if (_killGameAction == null)
		{
			Process[] processesByName = Process.GetProcessesByName("javaw");
			foreach (Process process in processesByName)
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch (Exception innerException)
				{
					throw new Exception("Failed to kill javaw process", innerException);
				}
			}
		}
		else
		{
			_killGameAction();
		}
		string cachePath = PathUtil.CachePath;
		if (Directory.Exists(cachePath))
		{
			FileUtil.DeleteDirectorySafe(cachePath);
		}
	}
}
