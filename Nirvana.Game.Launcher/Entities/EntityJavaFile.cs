using System.Runtime.InteropServices;
using Nirvana.Game.Launcher.Services.Java;
using Nirvana.Game.Launcher.Utils;
using NirvanaAPI.Utils;
using Serilog;

namespace Nirvana.Game.Launcher.Entities;

public class EntityJavaFile {
    private readonly bool _isNative; // 是否为 natives 库 [适用于低版本]

    private readonly string? _name = string.Empty; // 名称
    public readonly string? Url = string.Empty; // 下载地址

    private string _filePath = string.Empty; // 完整路径
    private string _filePath1 = string.Empty; // 相对路径，不保证是相对路径

    public EntityJavaFile(string path)
    {
        SetPath(path);
    }

    public EntityJavaFile(string path, string? url, string? name, bool isNative = false)
    {
        SetPath(path);
        Url = url;
        _name = name;
        _isNative = isNative;
    }

    public bool IsNative()
    {
        return _isNative || CommandService.GetRunOs().Any(os => EndsWithByName(":natives-" + os) || GetRunArch().Any(arch => EndsWithByName(":natives-" + os + "-" + arch)));
    }

    private static string[] GetRunArch()
    {
        return RuntimeInformation.ProcessArchitecture switch {
            Architecture.X86 => ["x86"],
            Architecture.X64 => ["x64"], // 排除作用
            _ => ["arm64"]
        };
    }

    public string GetPath()
    {
        return _filePath;
    }

    public string GetPath1()
    {
        return _filePath1;
    }

    public string GetPathSeparator()
    {
        return _filePath + PathUtil.PathSeparator;
    }

    public static string FixPath(string path)
    {
        var it = path.TrimEnd(' '); // 删除尾空格
        it = it.Replace('\\', Path.DirectorySeparatorChar); // 修复路径
        it = it.Replace('/', Path.DirectorySeparatorChar); // 修复路径
        return it;
    }

    private void SetPath(string path)
    {
        var it = FixPath(path);
        _filePath1 = it;
        _filePath = it.Contains(PathUtil.GameBasePath) ? it : Path.Combine(PathUtil.GameBasePath, ".minecraft", it);
    }

    public bool Equals(string path)
    {
        var it = FixPath(path);
        return _filePath1.Equals(it) || _filePath.Equals(it);
    }

    public bool Contains(string value)
    {
        var it = FixPath(value);
        return _filePath1.Contains(it) || it.Contains(_filePath1);
    }
    
    public bool StartsWith(string value)
    {
        var it = FixPath(value);
        return _filePath1.StartsWith(it) || _filePath.StartsWith(value);
    }

    private bool EndsWithByName(string value)
    {
        return _name != null && _name.EndsWith(value);
    }

    public static bool Contains(string value, string path)
    {
        var it = FixPath(path);
        var it1 = FixPath(value);
        return it.Contains(it1);
    }

    private bool IsNullOrEmptyByUrl()
    {
        return string.IsNullOrEmpty(Url) || Url.EndsWith('/');
    }

    private bool Exists()
    {
        return File.Exists(_filePath);
    }

    public static List<string> ToList(List<EntityJavaFile> files)
    {
        return files.Select(file => file.GetPath()).ToList();
    }

    public bool DownloadAuto()
    {
        if (Exists()) {
            return true;
        }

        if (IsNullOrEmptyByUrl()) {
            Log.Warning("jar {0} url is empty", GetPath1());
            return false;
        }

        DownloadAsync().Wait();
        return true;
    }

    private async Task DownloadAsync()
    {
        if (Url == null) {
            Log.Warning("jar {0} url is empty", GetPath1());
            return;
        }

        var url = Url;
        url = url.Replace("https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven");
        await DownloadUtil.DownloadAsync(url, GetPath());
    }
}