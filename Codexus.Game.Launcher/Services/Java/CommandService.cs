using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Utils;
using NirvanaAPI;
using NirvanaAPI.Utils;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Launch;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;
using WPFLauncherApi.Http;
using WPFLauncherApi.Utils;
using WPFLauncherApi.Utils.Cipher;

namespace Codexus.Game.Launcher.Services.Java;

public class CommandService {
    private static readonly JsonSerializerOptions Options = new() {
        AllowTrailingCommas = true
    };

    // " -Djava.library.path=123 "
    private static readonly List<string> StartList = [
        " --", // 0:--cp="123" | 1:--cp=123 | 2:--cp "123" | 3:--cp 123
        " -D", // 4: -Dcp="123" | 5: -Dcp=123 | 6: -Dcp "123" | 7: -Dcp 123
        " -" // 8: -cp="123" | 9: -cp=123 | 10: -cp "123" | 11: -cp 123
    ];

    private static readonly List<string> BetweenList = [
        "=\"",
        "=",
        " \"",
        " "
    ];

    private static readonly List<string> EndList = [
        "\"",
        "",
        "\"",
        ""
    ];

    private readonly JsonSerializerOptions _options = new() {
        // 关键设置：使用不转义的编码器
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private string _authToken = "";

    private string _cmd = "";

    private EnumGameVersion _gameVersion;

    private EntityLaunchGame? _launcherGame;

    private List<EntityJavaFile> _minecraft = []; // path, url

    private string _protocolVersion = "";

    private int _rpcPort = 11413;

    private string _uuid = "";

    private string _version = "";

    private string _workPath = "";

    public void Init(EnumGameVersion gameVersion, EntityLaunchGame entity, string dToken, string workPath, string uuid,
        int socketPort,
        string protocolVersion = "", int rpcPort = 11413)
    {
        _launcherGame = entity;
        _version = GameVersionUtil.GetGameVersionFromEnum(gameVersion);
        _gameVersion = gameVersion;
        _rpcPort = rpcPort;
        _uuid = uuid;
        _authToken = dToken;
        _workPath = workPath;
        _protocolVersion = protocolVersion;

        // windows 不需要修复 lwjgl
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // 获取原版信息
            var minecraft = X19Extensions.Bmcl.Api<Dictionary<string, JsonElement>>($"/version/{_version}/json").Result;
            if (minecraft != null) {
                _minecraft = BuildJarListBase(minecraft);
            } else {
                Log.Error("BmclApi returned null, version: {version}", _version);
            }
        }

        var path = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", _version, _version + ".json");
        if (!File.Exists(path)) {
            throw new Exception(
                "Game version JSON not found, please go to Setting to fix the game file and try again.");
        }

        var cfg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(options: Options,
            json: File.ReadAllText(path));

        if (cfg == null) {
            throw new Exception("Game version JSON deserialize failed.");
        }

        BuildCommand(cfg, _version, socketPort);
        InstallNatives().Wait();

        // 保存到文件，方便调试
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "command" + PathUtil.ScriptSuffix);
        Tools.SaveShellScript(scriptPath, GetJavaCommand()).Wait();
    }

    private async Task InstallNatives()
    {
        // windows 使用盒子资源
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return;
        }

        // 删除 linux/mac 下的 natives[win库]
        var nativesPath = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", _version, "natives");
        foreach (var native in Directory.GetFiles(nativesPath)) {
            if (native.EndsWith(".dll") || native.EndsWith(".so") || native.EndsWith(".dylib")) {
                File.Delete(native);
            }
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var javaFile in _minecraft) {
            // 不是 natives 库
            if (!javaFile.IsNative) {
                continue;
            }

            // 自动处理所需库
            if (!javaFile.DownloadAuto()) {
                continue;
            }

            await CompressionUtil.ExtractAsync(javaFile.GetPath(), nativesPath);
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
            CreateNoWindow = true, // 隐藏窗口
            WorkingDirectory = _workPath
        });
    }

    private static string GetJavaPath(EnumGameVersion gameVersion)
    {
        return Path.Combine(gameVersion >= EnumGameVersion.V_1_16 ? PathUtil.Jre17Path : PathUtil.Jre8Path,
            "bin", PathUtil.JavaExePath);
    }


    private static List<EntityJavaFile> BuildJarListBase(Dictionary<string, JsonElement> cfg)
    {
        // path, url
        var jarList = new List<EntityJavaFile>();

        if (!cfg.TryGetValue("libraries", out var libElement)) {
            throw new Exception("libraries not found");
        }

        foreach (var item in libElement.EnumerateArray()) {
            var isUse = true;

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
                    var name = nameElement.GetString();
                    if ("allow".Equals(action)) {
                        // name != os
                        // osx != osx
                        if (GetRunOs().Any(name1 => !name1.Equals(name))) {
                            isUse = false;
                            break;
                        }
                    } else if ("disallow".Equals(action)) {
                        // name == os
                        // osx == osx
                        if (GetRunOs().Any(name1 => name1.Equals(name))) {
                            isUse = false;
                            break;
                        }
                    }
                }
            }

            if (!isUse) {
                continue;
            }

            // downloads
            if (!item.TryGetProperty("downloads", out var downElement)) continue;

            // artifact
            if (downElement.TryGetProperty("artifact", out var artiElement)) {
                if (artiElement.TryGetProperty("path", out var pathElement)) {
                    var path = pathElement.GetString();
                    if (path != null) {
                        path = Path.Combine("libraries", path);
                        jarList = AddJarList(jarList, path,
                            artiElement.TryGetProperty("url", out var urlElement)
                                ? urlElement.GetString()
                                : string.Empty);
                    }
                }
            }

            // classifiers
            if (downElement.TryGetProperty("classifiers", out var classElement)) {
                if (item.TryGetProperty("natives", out var natives)) {
                    foreach (var osName1 in GetRunOs()) {
                        if (natives.TryGetProperty(osName1, out var nativeElement)) {
                            var osName = nativeElement.GetString();
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
                                            jarList = AddJarList(jarList, path,
                                                nativesElement.TryGetProperty("url", out var urlElement)
                                                    ? urlElement.GetString()
                                                    : string.Empty, true);
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
            Architecture.Arm64 => ["aarch_64", "arm64", "64"],
            Architecture.Arm => ["32"],
            Architecture.Armv6 => ["32"],
            Architecture.X86 => ["x86", "32"],
            _ => ["64", "x86_64"]
        };
    }

    // 已被禁用的架构
    private static string[] GetRunArchD()
    {
        return RuntimeInformation.ProcessArchitecture switch {
            Architecture.Arm64 => ["x86", "32", "x86_64"],
            Architecture.Arm or Architecture.Armv6 or Architecture.X86 => ["64", "x86_64", "aarch_64", "arm64", "x86"],
            _ => ["32", "aarch_64", "arm64", "x86"]
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

        foreach (var jar in from jar in BuildJarListsByName(cfg)
                 let isAdd = jarList.All(item => !item.Equals(jar))
                 where isAdd
                 select jar) {
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return ["osx", "macos"];
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ["windows"] : ["linux"];
    }

    // 所有操作系统
    private static string[] GetRunOsAll()
    {
        return ["windows", "osx", "macos", "linux"];
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
            jvmArguments = DeleteArguments("java.library.path", jvmArguments);
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
            var classPath1 = GetArguments("cp", jvmArguments);
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
            jvmArguments = UpdateArguments("cp", string.Join(PathUtil.PathSeparator, EntityJavaFile.ToList(classPaths)),
                jvmArguments, 10);
        }

        if (string.IsNullOrEmpty(jvmArguments)) {
            jvmArguments ??= "";
            jvmArguments = UpdateArguments("cp", string.Join(PathUtil.PathSeparator, EntityJavaFile.ToList(classPaths)),
                jvmArguments, 10);
        }

        if (cfg.TryGetValue("mainClass", out var mainClassElement)) {
            var mainClass = mainClassElement.GetString();
            // mainClass 不是空的 | 参数没有包含 mainClass
            if (!string.IsNullOrEmpty(mainClass) && !jvmArguments.Contains(mainClass)) {
                jvmArguments = AddArguments(jvmArguments, mainClass); // 添加 修复参数
            }
        }

        jvmArguments = jvmArguments.Replace("${library_directory}",
            Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries"));
        jvmArguments = UpdateArguments("libraryDirectory",
            Path.Combine(PathUtil.GameBasePath, ".minecraft", "libraries"), jvmArguments, 4);

        if (_launcherGame == null) {
            throw new Exception("No Launcher Game Found");
        }

        // 添加 验证信息
        var stringBuilder = new StringBuilder().Append(" -Xmx").Append(NirvanaConfig.Config.GameMemory).Append("M ")
            .Append(NirvanaConfig.Config.JvmArgs)
            .Append($" -DlauncherControlPort={socketPort}")
            .Append($" -DlauncherGameId={_launcherGame.GameId}")
            .Append($" -DuserId={_launcherGame.UserId}")
            .Append($" -DToken={_authToken}")
            .Append(" -DServer=RELEASE")
            .Append(AddNativePath());

        jvmArguments = AddArguments(stringBuilder.ToString(), jvmArguments); // 添加 修复参数

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

        jvmArguments = AddArguments(jvmArguments, BuildCommandExFix()); // 添加 修复参数

        minecraftArguments = UpdateArguments("gameDir", "\"${game_directory}\"", minecraftArguments, 3); // 修复错误内置
        minecraftArguments = UpdateArguments("assetsDir", "\"${assets_root}\"", minecraftArguments, 3); // 修复错误内置

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

        minecraftArguments = UpdateArguments("server", _launcherGame.ServerIp, minecraftArguments, 3);
        minecraftArguments = UpdateArguments("port", _launcherGame.ServerPort.ToString(), minecraftArguments, 3);

        minecraftArguments = UpdateArguments("userProperties", GetUserProperties(version), minecraftArguments, 3);
        minecraftArguments = UpdateArguments("userPropertiesEx", GetUserPropertiesEx(), minecraftArguments, 3);

        _cmd = AddArguments(jvmArguments, minecraftArguments);
    }

    private static List<EntityJavaFile> FilterFile(List<EntityJavaFile> classPaths)
    {
        var list = new List<EntityJavaFile>();
        var runArchList = GetRunArchD();
        var osNameList = GetRunOsAll();
        var osNameList1 = GetRunOs();
        foreach (var classPath in classPaths) {
            var isAdd = true;
            foreach (var osName in osNameList) {
                // -natives-windows.jar
                if (classPath.EndsWith("-natives-" + osName + ".jar")) {
                    // windows 不禁用 windows
                    if (osNameList1.All(osName1 => osName1 != osName)) {
                        isAdd = false;
                        break;
                    }
                }

                // -natives-windows-x86.jar
                if (runArchList.Select(arch => "-natives-" + osName + "-" + arch)
                    .Any(osNameArch => classPath.EndsWith(osNameArch + ".jar"))) {
                    isAdd = false;
                    break;
                }
            }

            if (isAdd) {
                list.Add(classPath);
            }
        }

        return list;
    }

    private static string AddArguments(string text, string tex1)
    {
        string value;
        // 最后1个字符是空格
        if (text.EndsWith(' ')) {
            value = text + tex1;
        } else {
            value = text + " " + tex1;
        }

        // 删除最后1个空格
        return value.TrimEnd(' ');
    }

    private static string GenArguments(string name, string value, int mode)
    {
        var index = 0;
        foreach (var start in StartList) {
            for (var i = 0; i < BetweenList.Count; i++) {
                if (index++ == mode) {
                    return start + name + BetweenList[i] + value + EndList[i];
                }
            }
        }

        return "";
    }

    private static string UpdateArguments(string name, string value, string text, int mode = 0)
    {
        var arguments = GetArguments(name, text, true);
        var genArguments = GenArguments(name, value, mode);
        // "  -a 1 " > " -a 1"
        return string.IsNullOrEmpty(arguments)
            ? (text.TrimEnd(' ') + " " + genArguments).TrimEnd(' ')
            : text.Replace(arguments, genArguments);
    }

    private static string DeleteArguments(string name, string text)
    {
        return UpdateArguments(name, string.Empty, text, -1);
    }

    /**
     * 获取参数值
     * @param name 参数名 cp
     * @param text 文本
     * @param complete 是否返回完整参数 -cp 123
     * @return 参数值
     */
    private static string GetArguments(string name, string text, bool complete = false)
    {
        return complete ? GetArguments1(name, text).Item1 : GetArguments1(name, text).Item2;
    }

    /**
     * 获取参数值
     * @param name 参数名 cp
     * @param text 文本
     * @return 完整参数 [-cp 123], 参数值 [123]
     */
    private static (string, string) GetArguments1(string name, string text)
    {
        foreach (var start in StartList) {
            for (var i = 0; i < BetweenList.Count; i++) {
                var between = BetweenList[i];
                var containText = start + name + between;
                if (!text.Contains(containText)) {
                    continue;
                }

                var value = Tools.GetBetweenStrings(text, containText, EndList[i] + " ");
                return (containText + value + EndList[i], value);
            }
        }

        return (string.Empty, string.Empty);
    }

    // 修复 -cp 路径
    private string ReplaceLib(string name, string text)
    {
        var sourceText = GetArguments1(name, text);
        if (string.IsNullOrEmpty(sourceText.Item1)) {
            return text;
        }

        var source = sourceText.Item2.Split(';');
        var combinedPaths = new StringBuilder();
        foreach (var pathSegment in source) {
            var fullPath = EntityJavaFile.FixPath(pathSegment);
            fullPath = fullPath.Replace(";", "");
            fullPath = fullPath.Replace(":", "");
            var fullPath1 = fullPath;
            fullPath += PathUtil.PathSeparator; // 修复 linux/mac 引用出错
            fullPath = Path.Combine(PathUtil.GameBasePath, ".minecraft", fullPath);
            fullPath1 = Path.Combine(PathUtil.GameBasePath, ".minecraft", fullPath1);

            if (!File.Exists(fullPath1)) {
                Log.Error("File not found: {fullPath}", fullPath1);
                continue;
            }

            // 修复 lwjgl
            if (!fullPath.Contains("lwjgl-") || _minecraft.Count == 0) {
                combinedPaths.Append(fullPath);
                continue;
            }

            // lwjgl-jemalloc-3.2.2.jar | lwjgl-3.2.2.jar
            // jemalloc-3.2.2 | 3.2.2
            var clasName = Tools.GetBetweenStrings(fullPath, "lwjgl-", ".jar");

            // "jemalloc-3.2.2" || ""
            clasName = clasName.Contains('-') ? clasName[(clasName.IndexOf('-') + 1)..] : "";
            // "jemalloc" || ""
            clasName = clasName.Contains('-') ? clasName[..clasName.IndexOf('-')] : "";

            var lwjglName = "lwjgl-" + clasName;

            // path, url
            var (file, index) = (null as EntityJavaFile, -1);
            foreach (var item in _minecraft) {
                if (item.Contains(lwjglName)) {
                    var index1 = item.GetPath().Length - lwjglName.Length;
                    if (index > index1 || index == -1) {
                        file = item;
                        index = index1;
                    }
                }
            }

            if (file == null) {
                Log.Error("File not found: {lwjglName}", lwjglName);
                continue;
            }

            // Log.Warning("{fullPath1} > {fullPath2}",fullPath1, file.GetPath());
            file.DownloadAuto();
            fullPath = file.GetPath();
            combinedPaths.Append(fullPath + PathUtil.PathSeparator);
        }

        return text.Replace(sourceText.Item1, " -" + name + " \"" + combinedPaths + "\"");
    }

    // 修复不同版本在不同系统出现问题
    private string BuildCommandExFix()
    {
        var stringBuilder = new StringBuilder();
        if (_gameVersion > EnumGameVersion.V_1_12_2) {
            // mac 高版本修复
            stringBuilder.Append("-XstartOnFirstThread ");
        }

        return stringBuilder.ToString();
    }

    private string AddNativePath()
    {
        var natives = Path.Combine(PathUtil.GameBasePath, ".minecraft", "versions", _version, "natives");

        // 避免 linux 出现权限问题
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var targetPath = natives + "/";
            var linkPath = "/tmp/fantnel-natives-" + _version;
            // 创建 natives 目录符号链。
            if (Directory.Exists(linkPath)) Directory.Delete(linkPath);
            Directory.CreateSymbolicLink(linkPath, targetPath);
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
            _launcherGame.UserId, 0, _rpcPort, RandomUtil.GetRandomString(32, "abcdefghijklmnopqrstuvwxyz"),
            _protocolVersion
        ];
        var text = string.Format(format, args);
        return "\"{" + text + "}\"";
    }
}