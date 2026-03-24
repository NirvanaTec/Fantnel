using System.Text.Json.Nodes;
using Nirvana.Public.Entities.Nirvana;
using Nirvana.WPFLauncher.Http;
using NirvanaAPI.Utils;
using Serilog;

namespace Nirvana.Public.Utils.Update;

public static class UpdateTools {
    // 检查更新
    public static async Task CheckUpdate(string[] args)
    {
        var update = 0; // 0:正常检查 1:不检查 2:已被检查
        if (args.Any(arg => arg == "--update_false")) {
            update = 2;
        }

        var threads = new List<Thread>();
        var updateConfigs = new List<EntityUpdateConfig1>();

        if (update == 0) {
            // 不检查 - 提醒
            // case 1:
            // {
            //     if (PublicProgram.Release)
            //     {
            //         for (var i = 0; i < 4; i++) Log.Warning("当前版本已取消自动更新，建议前往官网重新下载！");
            //
            //         Thread.Sleep(3000);
            //     }
            //
            //     break;
            // }
            // 正常检查
            if (PublicProgram.Release) {
                threads.Add(CheckUpdateThread(new EntityUpdateConfig {
                    Mode = PublicProgram.Mode + "." + PublicProgram.Arch,
                    Name = "Fantnel",
                    OnSuccess = array => {
                        updateConfigs.Add(new EntityUpdateConfig1 {
                            Name = "Fantnel",
                            Array = array,
                            Safe = true
                        });
                    }
                }));
            }
        }

        threads.Add(CheckUpdateThread(new EntityUpdateConfig {
            Mode = "ui." + (ConfigUtil.GetConfig()["themeValue"] ?? RestartTools.Get("default_skin_id", args,"nirvana")),
            OnSuccess = array => {
                updateConfigs.Add(new EntityUpdateConfig1 {
                    Array = array
                });
            }
        }));
        threads.Add(CheckUpdateThread(new EntityUpdateConfig {
            Mode = "static",
            OnSuccess = array => {
                updateConfigs.Add(new EntityUpdateConfig1 {
                    Array = array
                });
            }
        }));
        threads.Add(CheckUpdateThread(new EntityUpdateConfig {
            Mode = "static." + PublicProgram.Mode,
            OnSuccess = array => {
                updateConfigs.Add(new EntityUpdateConfig1 {
                    Array = array
                });
            }
        }));
        threads.Add(CheckUpdateThread(new EntityUpdateConfig {
            Mode = "static." + PublicProgram.Mode + "." + PublicProgram.Arch,
            FailureLog = false,
            OnSuccess = array => {
                updateConfigs.Add(new EntityUpdateConfig1 {
                    Array = array
                });
            }
        }));

        foreach (var thread in threads) {
            thread.Join();
        }

        foreach (var updateConfig in updateConfigs) {
            // 开始检查更新
            await CheckUpdate(updateConfig);
        }
    }

    private static Thread CheckUpdateThread(EntityUpdateConfig entityUpdateConfig)
    {
        var thread = new Thread(() => {
            var array = GetCheckUpdate(entityUpdateConfig.Mode, entityUpdateConfig.Name, entityUpdateConfig.FailureLog).Result;
            if (array != null) {
                entityUpdateConfig.OnSuccess?.Invoke(array);
            }
        });
        thread.Start();
        return thread;
    }

    /**
     * 获取检查更新消息
     * @param name 名称
     * @param safe 是否安全模式
     */
    private static async Task<JsonArray?> GetCheckUpdate(string mode, string name = "Resource", bool failureLog = true)
    {
        var jsonObj = await X19Extensions.Nirvana.Api<JsonObject>($"/api/fantnel/update/get?mode={mode}");
        if (jsonObj == null) {
            if (!failureLog) {
                return null;
            }

            Log.Error("{0}: {1}", name, mode);
            Log.Error("检查更新失败, 建议更新至最新版本!");
            return null;
        }

        var data = jsonObj["data"];
        if (data == null) {
            if (!failureLog) {
                return null;
            }

            Log.Error("{0}: {1}", name, mode);
            Log.Error("检查更新失败, 建议更新至最新版本!");
            return null;
        }

        return data.AsArray();
    }

    /**
     * 检查更新
     * @param name 名称
     * @param safe 是否安全模式
     */
    public static async Task<int> CheckUpdate(string mode, string name = "Resource", bool safe = false, bool failureLog = true)
    {
        var array = await GetCheckUpdate(mode, name, failureLog);
        if (array == null) {
            return -1;
        }

        var entityUpdateConfig1 = new EntityUpdateConfig1 {
            Name = name,
            Safe = safe,
            Array = array
        };
        await CheckUpdate(entityUpdateConfig1);
        return array.Count;
    }

    /**
     * 检查更新
     * @param name 名称
     * @param safe 是否安全模式
     */
    private static async Task CheckUpdate(EntityUpdateConfig1 entityUpdateConfig1)
    {
        await ThreadUpdateTools.CheckUpdate(entityUpdateConfig1);
    }
}