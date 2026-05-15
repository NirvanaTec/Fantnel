using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NirvanaAPI.Utils;
using Serilog;

namespace Nirvana.Public.Utils.Update;

public static class FileUtil {
    private static string GenerateUpdateScript(string command = "", string? filePath = null)
    {
        var updateScript = GenerateCopyScript(PathUtil.UpdaterPath, PathUtil.UpdaterBasePath);

        // 添加附加命令
        if (!string.IsNullOrEmpty(command)) {
            updateScript += command + "\n";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // Mac 终端结束后，启动进程, 无法显示进程窗口，因为 "进程已完成" 终端被关闭
            // Saving session...
            // ...copying shared history...
            // ...saving history...truncating history files...
            // ...completed.
            // [进程已完成]
            updateScript += "dotnet ";
            var commandLine = Environment.GetCommandLineArgs();
            if (!string.IsNullOrEmpty(filePath)) {
                commandLine[0] = "";
                updateScript += "\"" + filePath + "\" ";
            }

            for (var i = 0; i < commandLine.Length; i++) {
                var s = commandLine[i];
                if (i == 0) {
                    s = "\"" + s + "\"";
                }

                updateScript = updateScript + s + " ";
            }
        }

        // 替换换行符为 Windows 格式
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            updateScript = updateScript.Replace("\n", "\r\n");
        }

        Log.Information("更新脚本: \n{0}", updateScript);

        return updateScript;
    }

    private static string GenerateCopyScript(string tempDir, string targetDir)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"timeout /t 1 /nobreak\nxcopy /e /y /i \"{tempDir}\\*\" \"{targetDir}\"\n" : $"sleep 1\ncp -r \"{tempDir}/.\" \"{targetDir}\"\n";
    }

    /**
     * 安全重启更新
     * @param command 附加 脚本 命令 [更新\n + "Command" + \n启动]
     * @param filePath 完整文件路径
     */
    public static async Task SafeRestartUpdate(string command = "", string? filePath = null)
    {
        var scriptPath = PathUtil.ScriptPath;
        var commandScript = GenerateUpdateScript(command, filePath);
        await Tools.SaveShellScript(scriptPath, commandScript);
        // 启动更新脚本
        Process.Start(new ProcessStartInfo {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
            Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/C \"" + scriptPath + "\"" : scriptPath
        });
        // 避免占用文件
        Environment.Exit(0);
    }
}