using System;
using System.IO;
using System.Threading.Tasks;
using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using Serilog;

namespace Codexus.Game.Launcher.Services.Java;

public static class JreService
{
	public static async Task<bool> PrepareJavaRuntime()
	{
		string jreFile = Path.Combine(PathUtil.JavaPath, "Jre.7z");
		SyncProgressBarUtil.ProgressBar progress = new SyncProgressBarUtil.ProgressBar(100);
		IProgress<SyncProgressBarUtil.ProgressReport> uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(delegate(SyncProgressBarUtil.ProgressReport update)
		{
			progress.Update(update.Percent, update.Message);
		});
		await DownloadUtil.DownloadAsync("https://x19.gdl.netease.com/jre-v64-220420.7z", jreFile, delegate(uint p)
		{
			uiProgress.Report(new SyncProgressBarUtil.ProgressReport
			{
				Percent = (int)p,
				Message = "Downloading JRE"
			});
		});
		try
		{
			CompressionUtil.Extract7Z(jreFile, PathUtil.JavaPath, delegate(int p)
			{
				uiProgress.Report(new SyncProgressBarUtil.ProgressReport
				{
					Percent = p,
					Message = "Extracting JRE"
				});
			});
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to extract JRE");
			return false;
		}
		File.Delete(jreFile);
		SyncProgressBarUtil.ProgressBar.ClearCurrent();
		return true;
	}
}
