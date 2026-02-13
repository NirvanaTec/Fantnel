using System.Text;
using Serilog;

namespace NirvanaChat.Utils;

public class Utf8ByteReplacer {
    /// <summary>
    ///     在 UTF-8 编码的字节数组中查找子字符串的索引。
    /// </summary>
    /// <param name="haystack">要搜索的原始字节数组 (UTF-8 编码)。</param>
    /// <param name="needle">要查找的子字符串 (例如 "fengheng1314")。</param>
    /// <returns>找到的起始索引，如果未找到则返回 -1。</returns>
    public static int FindIndex(byte[] haystack, string needle)
    {
        // 将待查找的字符串编码为 UTF-8 字节数组
        var needleBytes = Encoding.UTF8.GetBytes(needle);
        return FindBytes(haystack, needleBytes);
    }

    /**
     * 查询字节数组中是否包含指定的子数组
     * 查询字节数组中是否包含指定的子数组
     */
    private static int FindBytes(byte[]? haystack, byte[]? needle)
    {
        if (needle == null || needle.Length == 0) {
            return -1;
        }

        if (haystack == null || needle.Length > haystack.Length) {
            return -1;
        }

        ReadOnlySpan<byte> haystackSpan = haystack.AsSpan();
        ReadOnlySpan<byte> needleSpan = needle.AsSpan();

        return haystackSpan.IndexOf(needleSpan);
    }

    /// <summary>
    ///     根据给定的索引，在 UTF-8 编码的字节数组中替换指定长度的内容。
    /// </summary>
    /// <param name="data">原始字节数组。</param>
    /// <param name="startIndex">开始替换的位置索引。</param>
    /// <param name="oldLength">被替换内容的原始字节长度。</param>
    /// <param name="newContent">用于替换的新字符串 (例如 "asdadsadas123")。</param>
    /// <returns>替换后的新字节数组。如果索引无效，则返回原数组。</returns>
    public static byte[] ReplaceAt(byte[] data, int startIndex, int oldLength, string newContent)
    {
        if (startIndex < 0 || startIndex > data.Length - oldLength) {
            Log.Error("错误：索引超出范围或数据为空。");
            return data; // 或者抛出异常
        }

        // 将新内容编码为 UTF-8 字节数组
        var newBytes = Encoding.UTF8.GetBytes(newContent);

        // 计算新数组的大小
        var result = new byte[data.Length - oldLength + newBytes.Length];

        // 复制索引之前的部分
        Array.Copy(data, 0, result, 0, startIndex);

        // 复制新的字节内容
        Array.Copy(newBytes, 0, result, startIndex, newBytes.Length);

        // 复制索引之后的部分
        Array.Copy(data, startIndex + oldLength, result, startIndex + newBytes.Length,
            data.Length - startIndex - oldLength);

        return result;
    }
}