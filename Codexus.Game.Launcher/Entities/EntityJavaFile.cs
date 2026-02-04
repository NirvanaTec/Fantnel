using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codexus.Game.Launcher.Utils;
using NirvanaAPI.Utils;
using Serilog;

namespace Codexus.Game.Launcher.Entities;

public class EntityJavaFile {
    
    private string _filePath; // 完整路径
    private string _filePath1; // 相对路径，不保证是相对路径
    public string Url = string.Empty; // 下载定制
    public bool IsNative = false; // 是 Native 文件, 不保证是 Native 文件

    public EntityJavaFile(string path)
    {
        SetPath(path);
    }

    public string GetPath()
    {
        return _filePath;
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
    
    public bool Contains(string name)
    {
        return _filePath1.Contains(name);
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
    
    public bool EndsWith(string path)
    {
        return _filePath1.EndsWith(path);
    }

    public bool DownloadAuto()
    {
        if (Exists()) {
            return true;
        }
        if (IsNullOrEmptyByUrl()) {
            Log.Warning("jar {ItemKey} url is empty", GetPath());
            return false;
        }
        DownloadAsync().Wait();
        return true;
    }

    private async Task DownloadAsync()
    {
        var url = Url;
        url = url.Replace("https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven");
        await DownloadUtil.DownloadAsync(url, GetPath());
    }
    
}