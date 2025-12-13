using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NirvanaPublic.Entities.NEL;

namespace NirvanaPublic.Utils;

public static class Tools
{

    public static (T[], string) GetValueOrDefault<T>(string fileName)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", fileName);
        List<T>? entity = [];
        if (!File.Exists(path))
        {
            return (entity.ToArray(), path);
        }

        var json = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            // 异常格式处理
            entity = JsonSerializer.Deserialize<List<T>>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        // 处理空数组
        entity ??= [];
        // 过滤空值
        foreach (var item in entity.ToArray())
        {
            if (item == null)
            {
                entity.Remove(item);
            }
        }

        return (entity.ToArray(), path);
    }


    /**
     * 计算文件的MD5哈希值
     * @param filePath 文件绝对路径
     * @return 32位小写MD5字符串
     */
    public static string GetFileMd5(string filePath)
    {
        // 使用using语句确保资源释放
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        // 计算哈希值并转换为十六进制字符串
        var hashBytes = md5.ComputeHash(stream);
        // 去除分隔符并转为小写（符合通用MD5格式）
        return Convert.ToHexStringLower(hashBytes);
    }
    
}

