using System.Runtime.InteropServices;
using Nirvana.Public.Entities.EntitiesUpdate;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using Serilog;

namespace Nirvana.Public.Utils.Update;

public static class UpdateTools {
    
    // 检查更新
    public static async Task CheckUpdate(string[] args)
    {
        if (InfoManager.FantnelInfo == null) {
            Log.Error("无法连接至服务器！");
            Thread.Sleep(6000);
            Environment.Exit(1);
            return;
        }
        
        if (!"1.0.0".Equals(InfoManager.FantnelInfo.UpdateVersions)) {
            Log.Error("当前版本已被禁用，请前往官网重新下载！");
            Thread.Sleep(6000);
            Environment.Exit(1);
            return;
        }
        
        // --- Fantnel ---
        var update = 0; // 0:正常检查 1:不检查 2:已被检查
        if (args.Any(arg => arg == "--update_false")) {
            update = 2;
        }

        // 正常检查
        if (update == 0 && PublicProgram.Release) {
            await new EntityUpdate {
                Mode = PathUtil.SystemArch,
                Name = "Fantnel",
                SafeMode = true,
                Command = ""
            }.CheckUpdateSafe();
        }
        
        // --- Fantnel UI ---
        update = 0; // 0:正常检查 1:不检查 2:已被检查
        if (args.Any(arg => arg == "--update_ui_false")) {
            update = 2;
        }

        if (update == 0) {
            await new EntityUpdate {
                Mode = "ui." + ConfigUtil.GetConfig("themeValue", RestartTools.Get("default_skin_id", args, "nirvana")),
                Name = "Fantnel UI",
            }.CheckUpdateSafe();
        }
        
        // --- Static ---
        update = 0; // 0:正常检查 1:不检查 2:已被检查
        if (args.Any(arg => arg == "--update_static_false")) {
            update = 2;
        }

        if (update == 0) {
            await new EntityUpdate {
                Mode = "static",
            }.CheckUpdateSafe();
        }
        
        // --- Static System ---
        update = 0; // 0:正常检查 1:不检查 2:已被检查
        if (args.Any(arg => arg == "--update_static_system_false")) {
            update = 2;
        }

        if (update == 0) {
            await new EntityUpdate {
                Mode = "static." + PathUtil.DetectOperating,
                Name = "Resource System",
            }.CheckUpdateSafe();
        }
        
        // --- Static Linux System ---
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            update = 0; // 0:正常检查 1:不检查 2:已被检查
            if (args.Any(arg => arg == "--update_static_linux_system_false")) {
                update = 2;
            }

            if (update == 0) {
                await new EntityUpdate {
                    Mode = "static." + PathUtil.SystemArch,
                    Name = "Resource Linux",
                }.CheckUpdateSafe();
            }
        }

    }
    

}