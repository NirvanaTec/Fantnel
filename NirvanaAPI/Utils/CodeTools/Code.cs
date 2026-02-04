using System.Text.Json;
using NirvanaAPI.Entities;

namespace NirvanaAPI.Utils.CodeTools;

public static class Code {
    public static EntityResponse<string> ToJson(Exception t)
    {
        var json = new EntityResponse<string> {
            Code = -1,
            Msg = t.Message
        };
        return json;
    }

    public static string ToJson(ErrorCode code, object? data = null)
    {
        return JsonSerializer.Serialize(ToJson1(code, data));
    }

    public static string ToJson<T>(EntityResponse<T> json)
    {
        return JsonSerializer.Serialize(json);
    }

    public static EntityResponse<object> ToJson1(ErrorCode code, object? data = null)
    {
        return ToJson1(code, new EntityResponse<object>(), data);
    }

    private static EntityResponse<object> ToJson1(ErrorCode code, EntityResponse<object> json, object? data = null)
    {
        json.Code = (int)code;
        json.Msg = GetMessage(code);
        json.Data = data;
        return json;
    }

    public static string GetMessage(ErrorCode code)
    {
        return code switch {
            ErrorCode.Failure => "失败",
            ErrorCode.Success => "成功",
            ErrorCode.FileExists => "文件不存在",
            ErrorCode.FileFormat => "文件是错误格式",
            ErrorCode.GetExecutingAssemblyLocation => "无法获取当前执行程序集",
            ErrorCode.ServicesNotInitialized => "Services 服务未初始化",
            ErrorCode.AccountError => "账号错误或异常",
            ErrorCode.PasswordError => "密码错误或异常",
            ErrorCode.EmailOrPasswordError => "邮箱或密码错误",
            ErrorCode.LoginError => "登录出现未知错误",
            ErrorCode.LoadAccountError => "识别账号时出现异常",
            ErrorCode.DirectoryCreateError => "创建目录失败",
            ErrorCode.CaptchaError => "验证码错误",
            ErrorCode.NotFound => "没有找到",
            ErrorCode.IdError => "ID 错误",
            ErrorCode.LogInNot => "没有登录",
            ErrorCode.ServerInNot => "未知的服务器",
            ErrorCode.NameInNot => "名称不正确",
            ErrorCode.DetailError => "详细详细获取失败",
            ErrorCode.AddressError => "地址获取失败",
            ErrorCode.NotFoundName => "没有找到名称",
            ErrorCode.ModsError => "Mods 错误",
            ErrorCode.PluginNotFound => "插件不存在",
            ErrorCode.FormatError => "格式异常",
            ErrorCode.CaptchaNot => "没有验证",
            ErrorCode.CrcSaltNotSet => "CRC 盐值未设置",
            ErrorCode.RestartFailed => "重启失败",
            _ => "未知错误"
        };
    }
}