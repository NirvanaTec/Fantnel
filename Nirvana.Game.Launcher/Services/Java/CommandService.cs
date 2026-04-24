using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Nirvana.Game.Launcher.Entities;
using Nirvana.Game.Launcher.Utils;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Launch;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using Nirvana.WPFLauncher.Http;
using Nirvana.WPFLauncher.Utils;
using Nirvana.WPFLauncher.Utils.Cipher;
using NirvanaAPI;
using NirvanaAPI.Utils;
using Serilog;
using TokenUtil = Nirvana.WPFLauncher.Utils.Cipher.TokenUtil;

namespace Nirvana.Game.Launcher.Services.Java;

public class CommandService {
    private static readonly JsonSerializerOptions Options = new() {
        AllowTrailingCommas = true
    };

    private readonly JsonSerializerOptions _options = new() {
        // 关键设置：使用不转义的编码器
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private string _cmd = "";

    private EnumGameVersion _gameVersion;

    private EntityLaunchGame? _launcherGame;

    private List<EntityJavaFile> _minecraft = []; // path, url

    private string _protocolVersion = "";

    private int _rpcPort = 11413;

    private string _uuid = "";

    private string _version = "";

    private string _workPath = "";

    public void Init(EnumGameVersion gameVersion, EntityLaunchGame entity, string workPath, string uuid,
        int socketPort,
        string protocolVersion = "", int rpcPort = 11413)
    {
        _launcherGame = entity;
        _version = GameVersionUtil.GetGameVersionFromEnum(gameVersion);
        _gameVersion = gameVersion;
        _rpcPort = rpcPort;
        _uuid = uuid;
        _workPath = workPath;
        _protocolVersion = protocolVersion;

        // windows 不需要修复 lwjgl
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // 获取原版信息
            var minecraft = X19Extensions.Bmcl.Api<Dictionary<string, JsonElement>>($"/version/{_version}/json").Result;
            if (minecraft != null) {
                _minecraft = BuildJarListBase(minecraft);
            } else {
                Log.Error("BmclApi returned null, version: {0}", _version);
            }
        }

        var path = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", _version, _version + ".json");
        if (!File.Exists(path)) {
            throw new Exception("Game version JSON not found, please go to Setting to fix the game file and try again.");
        }

        var cfg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(options: Options,
            json: File.ReadAllText(path));

        if (cfg == null) {
            throw new Exception("Game version JSON deserialize failed.");
        }

        BuildCommand(cfg, _version, socketPort);
        // 修复 natives
        InstallNatives().Wait();

        // 保存到文件，方便调试
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "command" + PathUtil.ScriptSuffix);
        Tools.SaveShellScript(scriptPath, GetJavaCommand()).Wait();
        
        var optionsPath = Path.Combine(_workPath, "options.txt");
        if (!File.Exists(optionsPath)) {
            File.WriteAllText(optionsPath, "guiScale:2\nlang:zh_cn\nmaxFps:120\n");
        }
    }

    private async Task InstallNatives()
    {
        if (_minecraft.Count == 0) {
            return;
        }

        // 删除 linux/mac 下的 natives[win库]
        var nativesPath = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", _version, "natives");
        Directory.Delete(nativesPath, true);

        foreach (var item in _minecraft.Where(item => item.IsNative && item.DownloadAuto())) {
            await CompressionUtil.ExtractAsync(item.GetPath(), nativesPath);
        }
    }

    // 生成启动参数 【独立运行】
    private string GetJavaCommand()
    {
        return "cd \"" + _workPath + "\"" + "\n" + GetJavaPath(_gameVersion) + _cmd;
    }

    public Process? StartGame()
    {
        var javaPath = GetJavaPath(_gameVersion);
        FileUtil.SetUnixFilePermissions(javaPath); // 添加 Java 权限
        return Process.Start(new ProcessStartInfo(javaPath, _cmd) {
            UseShellExecute = false,
            WorkingDirectory = _workPath
        });
    }

    private static string GetJavaPath(EnumGameVersion gameVersion)
    {
        return Path.Combine(gameVersion >= EnumGameVersion.V_1_16 ? PathUtil.Jre17Path : PathUtil.Jre8Path,
            "bin", PathUtil.JavaExePath);
    }


    private static bool CheckRules(JsonElement item)
    {
        // rules
        if (item.TryGetProperty("rules", out var rulesElement)) {
            foreach (var ruleItem in rulesElement.EnumerateArray()) {
                if (!ruleItem.TryGetProperty("action", out var actionElement)) {
                    continue;
                }

                if (!ruleItem.TryGetProperty("os", out var osElement)) {
                    continue;
                }

                if (!osElement.TryGetProperty("name", out var nameElement)) {
                    continue;
                }

                var action = actionElement.GetString();
                var name = nameElement.GetString(); // 系统名称

                switch (action) {
                    // name != os
                    // osx != osx
                    case "allow" when GetRunOs().Any(name1 => name1.Equals(name)):
                        return true;
                    // name == os
                    // osx == osx
                    case "disallow" when GetRunOs().Any(name1 => name1.Equals(name)):
                        return false;
                }
            }
        }

        return true;
    }

    private static List<EntityJavaFile> BuildJarListBase(Dictionary<string, JsonElement> cfg)
    {
        // path, url
        var jarList = new List<EntityJavaFile>();

        if (!cfg.TryGetValue("libraries", out var libElement)) {
            throw new Exception("libraries not found");
        }

        foreach (var item in libElement.EnumerateArray().Where(CheckRules)) {
            // downloads
            if (!item.TryGetProperty("downloads", out var downElement)) {
                continue;
            }

            // artifact
            if (downElement.TryGetProperty("artifact", out var artiElement)) {
                if (artiElement.TryGetProperty("path", out var pathElement)) {
                    var path = pathElement.GetString();
                    if (path != null) {
                        path = Path.Combine("libraries", path);
                        jarList = AddJarList(jarList, path, artiElement.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : string.Empty);
                    }
                }
            }

            // classifiers
            if (downElement.TryGetProperty("classifiers", out var classElement)) {
                if (item.TryGetProperty("natives", out var natives)) {
                    foreach (var osName1 in GetRunOs()) {
                        if (natives.TryGetProperty(osName1, out var nativeElement)) {
                            // "windows": "natives-windows"
                            var osName = nativeElement.GetString(); // natives-windows
                            if (string.IsNullOrEmpty(osName)) {
                                continue;
                            }
                            var runArch = GetRunArch();
                            foreach (var archName in runArch) {
                                var osNameArch = osName.Replace("${arch}", archName);
                                if (classElement.TryGetProperty(osNameArch, out var nativesElement)) {
                                    if (nativesElement.TryGetProperty("path", out var path1Element)) {
                                        var path = path1Element.GetString();
                                        if (path != null) {
                                            path = Path.Combine("libraries", path);
                                            jarList = AddJarList(jarList, path, nativesElement.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : string.Empty, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return jarList;
    }

    private static string[] GetRunArch()
    {
        return RuntimeInformation.ProcessArchitecture switch {
            // Architecture.Arm64 => ["aarch_64"],
            // Architecture.Arm => ["aarch_64"],
            // Architecture.Armv6 => ["aarch_64"],
            Architecture.X86 => ["32"],
            Architecture.X64 => ["64"],
            _ => ["aarch_64"]
        };
    }

    private static List<EntityJavaFile> AddJarList(List<EntityJavaFile> jarList, string path, string? url,
        bool isNative = false)
    {
        if (jarList.Any(jar => jar.Equals(path))) {
            // 获取已存在的值的url
            var value = string.Empty;
            foreach (var jar in jarList.Where(jar => jar.Equals(path))) {
                value = jar.Url;
            }

            // 已存在的值为空，移除
            if (string.IsNullOrEmpty(value)) {
                jarList.RemoveAll(jar => jar.Equals(path));
            } else {
                // 已存在的值不为空，跳过
                return jarList;
            }
        }

        jarList.Add(new EntityJavaFile(path) {
            Url = url,
            IsNative = isNative
        });
        return jarList;
    }

    private static List<string> BuildJarListsByName(Dictionary<string, JsonElement> cfg)
    {
        var jarList = new List<string>();
        if (cfg.TryGetValue("libraries", out var value)) {
            foreach (var item in value.EnumerateArray()) {
                if (!item.TryGetProperty("name", out var value2)) {
                    continue;
                }

                var array = value2.GetString()?.Split(':');
                if (array is not { Length: >= 3 } || array[1].Contains("platform")) {
                    continue;
                }

                var path = array[0].Replace('.', Path.DirectorySeparatorChar);
                var path2 = array[1] + "-" + array[2] + ".jar";
                jarList.Add(Path.Combine("libraries", path, array[1], array[2], path2));
            }
        }

        return jarList;
    }

    private static List<EntityJavaFile> BuildJarLists(Dictionary<string, JsonElement> cfg, string version)
    {
        // path, url
        var jarList = BuildJarListBase(cfg);

        // 修复 1.8.9 奇葩 libraries
        foreach (var jar in BuildJarListsByName(cfg).Where(jar => jarList.All(item => !item.Equals(jar)))) {
            jarList.Add(new EntityJavaFile(jar));
        }

        // \versions\1.8.9\1.8.9.jar
        var verValue = Path.Combine("versions", version, version + ".jar");
        jarList.Add(new EntityJavaFile(verValue));

        // 自动处理所需库
        return jarList.Where(item => item.DownloadAuto()).ToList();
    }

    private static string[] GetRunOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return ["windows"];
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return ["osx", "macos"];
        }

        return ["linux"];
    }

    private void BuildCommand(Dictionary<string, JsonElement> cfg, string version, int socketPort)
    {
        var jvmArguments = "";
        if (cfg.TryGetValue("jvm_arguments", out var jvmArguments1)) {
            jvmArguments = jvmArguments1.GetString();
        }

        var classPaths = BuildJarLists(cfg, _version);

        if (!string.IsNullOrEmpty(jvmArguments)) {
            // 修复 linux/mac 冲突
            jvmArguments = GameArgumentsUtil.DeleteArguments("java.library.path", jvmArguments, CommandMode.Mode5, CommandMode.Mode6);
            // 修复 模块路径 错误
            jvmArguments = ReplaceLib("p", jvmArguments);
            // // 修复 linux/mac 冲突
            // if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
            //     // 避免 linux 使用 win 库
            //     genClassPath = true;
            //     jvmArguments = DeleteArguments("cp", jvmArguments);
            // }
            jvmArguments = ReplaceLib("cp", jvmArguments);

            // 而外 lib 路径
            var classPath1 = GameArgumentsUtil.GetArguments("cp", jvmArguments, false, CommandMode.Mode12, CommandMode.Mode13);
            var classPathList = classPath1.Split(PathUtil.PathSeparator);

            // 过滤 重复路径
            foreach (var path in classPathList) {
                if (string.IsNullOrEmpty(path)) {
                    continue;
                }

                var isAdd = classPaths.All(classPath => !classPath.Contains(path));
                if (isAdd) {
                    classPaths.Add(new EntityJavaFile(path));
                }
            }

            classPaths = FilterFile(classPaths);
            jvmArguments = GameArgumentsUtil.UpdateArguments("cp", string.Join(PathUtil.PathSeparator, EntityJavaFile.ToList(classPaths)), jvmArguments,  CommandMode.Mode12, CommandMode.Mode13);
        }

        if (string.IsNullOrEmpty(jvmArguments)) {
            jvmArguments ??= "";
            jvmArguments = GameArgumentsUtil.UpdateArguments("cp", string.Join(PathUtil.PathSeparator, EntityJavaFile.ToList(classPaths)), jvmArguments,  CommandMode.Mode12, CommandMode.Mode13);
        }

        if (cfg.TryGetValue("mainClass", out var mainClassElement)) {
            var mainClass = mainClassElement.GetString();
            // mainClass 不是空的 | 参数没有包含 mainClass
            if (!string.IsNullOrEmpty(mainClass) && !jvmArguments.Contains(mainClass)) {
                jvmArguments = GameArgumentsUtil.AddArguments(jvmArguments, mainClass); // 添加 修复参数
            }
        }

        jvmArguments = jvmArguments.Replace("${library_directory}",
            Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries"));
        jvmArguments = GameArgumentsUtil.UpdateArguments("libraryDirectory",
            Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries"), jvmArguments, CommandMode.Mode5, CommandMode.Mode6);
        jvmArguments = GameArgumentsUtil.DeleteArguments("Xmx", jvmArguments, CommandMode.Mode14);
        
        if (_launcherGame == null) {
            throw new Exception("No Launcher Game Found");
        }

        // 添加 验证信息
        var stringBuilder = new StringBuilder().Append(" -Xmx").Append(NirvanaConfig.GetString("gameMemory")).Append("M ")
            .Append(NirvanaConfig.GetString("jvmArgs"))
            .Append($" -DlauncherControlPort={socketPort}")
            .Append($" -DlauncherGameId={_launcherGame.GameId}")
            .Append($" -DuserId={_launcherGame.Account.GetUserId()}")
            .Append($" -DToken={TokenUtil.GenerateEncryptToken(_launcherGame.Account.GetToken())}")
            .Append(" -DServer=RELEASE")
            .Append(AddNativePath());

        jvmArguments = GameArgumentsUtil.AddArguments(stringBuilder.ToString(), jvmArguments); // 添加 修复参数

        // 启动参数
        var minecraftArguments = "";
        if (cfg.TryGetValue("parameter_arguments", out var minecraftArguments1)) {
            minecraftArguments = minecraftArguments1.GetString();
        }

        if (string.IsNullOrEmpty(minecraftArguments)) {
            minecraftArguments = cfg.GetValueOrDefault("minecraftArguments").GetString();
        }

        // 没有启动参数
        if (string.IsNullOrEmpty(minecraftArguments)) {
            throw new ArgumentNullException(minecraftArguments);
        }

        jvmArguments = GameArgumentsUtil.AddArguments(jvmArguments, BuildCommandExFix()); // 添加 修复参数

        minecraftArguments = GameArgumentsUtil.UpdateArguments("gameDir", "${game_directory}", minecraftArguments, CommandMode.Mode2, CommandMode.Mode3); // 修复错误内置
        minecraftArguments = GameArgumentsUtil.UpdateArguments("assetsDir", "${assets_root}", minecraftArguments, CommandMode.Mode2, CommandMode.Mode3); // 修复错误内置

        minecraftArguments = minecraftArguments.Replace("${game_directory}", _workPath);
        minecraftArguments = minecraftArguments.Replace("--userType ${user_type}", string.Empty);
        minecraftArguments = minecraftArguments.Replace("${version_name}", version);
        minecraftArguments = minecraftArguments.Replace("${auth_player_name}", _launcherGame.RoleName);
        minecraftArguments = minecraftArguments.Replace("${auth_uuid}", _uuid);
        minecraftArguments = minecraftArguments.Replace("--versionType ${version_type}", string.Empty);
        minecraftArguments = minecraftArguments.Replace("${assets_root}",
            Path.Combine(PathUtil.GameBasePath, ".minecraft", "assets"));
        minecraftArguments = minecraftArguments.Replace("${assets_index_name}", version);

        minecraftArguments = minecraftArguments.Replace("${auth_access_token}",
            _gameVersion >= EnumGameVersion.V_1_18 ? "0" : RandomUtil.GetRandomString(32, "ABCDEF0123456789"));

        minecraftArguments = GameArgumentsUtil.UpdateArguments("server", _launcherGame.ServerIp, minecraftArguments, CommandMode.Mode3, CommandMode.Mode13);
        minecraftArguments = GameArgumentsUtil.UpdateArguments("port", _launcherGame.ServerPort.ToString(), minecraftArguments, CommandMode.Mode3, CommandMode.Mode13);

        minecraftArguments = GameArgumentsUtil.UpdateArguments("userProperties", GetUserProperties(version), minecraftArguments, CommandMode.Mode3, CommandMode.Mode13);
        minecraftArguments = GameArgumentsUtil.UpdateArguments("userPropertiesEx", GetUserPropertiesEx(), minecraftArguments, CommandMode.Mode3, CommandMode.Mode13);

        _cmd = GameArgumentsUtil.AddArguments(jvmArguments, minecraftArguments);
    }

    private List<EntityJavaFile> FilterFile(List<EntityJavaFile> classPaths)
    {
        // 是 natives 文件，不用添加
        return _minecraft.Count <= 0 ? classPaths :
            classPaths.Where(classPath => !classPath.Contains("-natives-")).ToList();
    }

    // 修复 -cp 路径
    private string ReplaceLib(string name, string text)
    {
        var sourceText = GameArgumentsUtil.GetArguments1(name, text, CommandMode.Mode12, CommandMode.Mode13);
        if (string.IsNullOrEmpty(sourceText.Item1)) {
            return text;
        }

        var source = sourceText.Item2.Split(';');
        var combinedPaths = new StringBuilder();
        foreach (var pathSegment in source) {
            var fullPath = pathSegment.Replace(";", "");
            fullPath = fullPath.Replace(":", "");
            fullPath = EntityJavaFile.FixPath(fullPath); // 有分割符

            var filePath = fullPath; // 不完整路径

            var fullPath1 = filePath; // 无分割符
            fullPath1 = Path.Combine(PathUtil.GameBasePath, ".minecraft", fullPath1);

            fullPath = fullPath1 + PathUtil.PathSeparator; // 修复 linux/mac 引用出错

            if (!File.Exists(fullPath1)) {
                Log.Error("File not found: {0}", fullPath1);
                continue;
            }

            // 是 lwjgl 文件，不用添加
            if (_minecraft.Count > 0 && filePath.Contains(EntityJavaFile.FixPath("org/lwjgl/"))) {
                continue;
            }

            combinedPaths.Append(fullPath);
        }

        if (_minecraft.Count == 0) {
            return text.Replace(sourceText.Item1, " -" + name + " \"" + combinedPaths + "\"");
        }

        // 修复 lwjgl
        foreach (var item in _minecraft.Where(item => item.StartsWith("org/lwjgl/") && item.DownloadAuto())) {
            combinedPaths.Append(item.GetPathSeparator());
        }

        return text.Replace(sourceText.Item1, " -" + name + " \"" + combinedPaths + "\"");
    }

    // 修复不同版本在不同系统出现问题
    private string BuildCommandExFix()
    {
        var stringBuilder = new StringBuilder();
        if (_gameVersion > EnumGameVersion.V_1_12_2) {
            // Mac 高版本修复
            stringBuilder.Append("-XstartOnFirstThread ");
        }

        return stringBuilder.ToString();
    }

    private string AddNativePath()
    {
        var natives = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", _version, "natives");

        // 避免 linux 出现权限问题
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var linkPath = "/tmp/fantnel-natives-" + _version;
            // 创建 natives 目录符号链。
            Tools.CreateSymbolicLink(linkPath, natives);
            natives = linkPath;
        }

        var runtime = Path.Combine(natives, "runtime");
        return $" -Djava.library.path=\"{natives}\" -Druntime_path=\"{runtime}\" ";
    }

    private string GetUserPropertiesEx(EnumGType t = EnumGType.NetGame)
    {
        var jsonContent = JsonSerializer.Serialize(new EntityUserPropertiesEx {
            GameType = (int)t,
            Channel = "netease",
            TimeDelta = 0,
            IsFilter = true,
            LauncherVersion = _protocolVersion
        });

        // 引号 转换成 \"
        return JsonSerializer.Serialize(jsonContent, _options);
    }

    private string GetUserProperties(string version)
    {
        if (_launcherGame == null) {
            throw new Exception("No Launcher Game Found");
        }

        var format = version == "1.7.10"
            ? "\"uid\":[{0}],\"gameid\":[{1}],\"launcherport\":[{2}],\\\"filterkey\\\":[\\\"{3}\\\",\\\"0\\\"],\\\"filterpath\\\":[\\\"\\\",\\\"0\\\"],\\\"timedelta\\\":[0,0],\\\"launchversion\\\":[\\\"{3}\\\",\\\"0\\\"]"
            : "\\\"uid\\\":[{0},0],\\\"gameid\\\":[{1},0],\\\"launcherport\\\":[{2},0],\\\"filterkey\\\":[\\\"{3}\\\",\\\"0\\\"],\\\"filterpath\\\":[\\\"\\\",\\\"0\\\"],\\\"timedelta\\\":[0,0],\\\"launchversion\\\":[\\\"{4}\\\",\\\"0\\\"]";
        object?[] args = [
            _launcherGame.Account.GetUserId(), 0, _rpcPort, RandomUtil.GetRandomString(32, "abcdefghijklmnopqrstuvwxyz"),
            _protocolVersion
        ];
        var text = string.Format(format, args);
        return "\"{" + text + "}\"";
    }
}