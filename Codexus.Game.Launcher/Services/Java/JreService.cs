using Codexus.Game.Launcher.Utils;
using Codexus.Game.Launcher.Utils.Progress;
using NirvanaAPI.Utils;
using Serilog;

namespace Codexus.Game.Launcher.Services.Java;

public static class JreService {
    public static async Task<bool> PrepareJavaRuntime()
    {
        var jreFile = Path.Combine(PathUtil.JavaPath, "jre-v64.7z");
        var progress = new SyncProgressBarUtil.ProgressBar();
        var uiProgress = new SyncCallback<SyncProgressBarUtil.ProgressReport>(update => {
            progress.Update(update.Percent, update.Message);
        });
        await DownloadUtil.DownloadAsync("https://x19.gdl.netease.com/jre-v64-220420.7z", jreFile, p => {
            uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                Percent = p,
                Message = "Downloading JRE"
            });
        });
        try {
            await CompressionUtil.ExtractAsync(jreFile, PathUtil.JavaPath, p => {
                uiProgress.Report(new SyncProgressBarUtil.ProgressReport {
                    Percent = p,
                    Message = "Extracting JRE"
                });
            });
        } catch (Exception ex) {
            Log.Error(ex, "Failed to extract JRE");
            return false;
        }

        File.Delete(jreFile);
        SyncProgressBarUtil.ProgressBar.ClearCurrent();
        return true;
    }
}