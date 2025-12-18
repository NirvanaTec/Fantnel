using System.Text.Json;
using NirvanaPublic.Entities.Nirvana;

namespace NirvanaPublic.Utils.ViewLogger;

public static class Code
{
    public enum ErrorCode
    {
        Failure = 0,
        Success = 1,
        FileExists = 2,
        FileFormat = 3,
        GetExecutingAssemblyLocation = 4,
        ServicesNotInitialized = 5,
        AccountError = 6,
        PasswordError = 7,
        EmailOrPasswordError = 8,
        LoginError = 9,
        LoadAccountError = 10,
        DirectoryCreateError = 11,
        CaptchaError = 12,
        NotFound = 13,
        IdError = 14,
        LogInNot = 15,
        ServerInNot = 16,
        NameInNot = 17,
        DetailError = 18,
        AddressError = 19,
        NotFoundName = 20,
        ModsError = 21,
        PluginNotFound = 22,
        FormatError = 23,
        CaptchaNot = 24
    }

    public static EntityResponse<string> ToJson(Exception t)
    {
        var json = new EntityResponse<string>
        {
            Code = -1,
            Msg = t.Message
        };
        return json;
    }

    public static string ToJson(ErrorCode code, object? data = null)
    {
        return JsonSerializer.Serialize(ToJson1(code, new EntityResponse<object>(), data));
    }

    public static string ToJson<T>(EntityResponse<T> json)
    {
        return JsonSerializer.Serialize(json);
    }

    public static EntityResponse<object> ToJson1(ErrorCode code, object? data = null)
    {
        return ToJson1(code, new EntityResponse<object>(), data);
    }

    public static EntityResponse<object> ToJson1(ErrorCode code, EntityResponse<object> json, object? data = null)
    {
        json.Code = (int)code;
        json.Msg = GetMessage(code);
        json.Data = data;
        return json;
    }

    private static string GetMessage(ErrorCode code)
    {
        return code switch
        {
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
            _ => "未知错误"
        };
    }

    public class ErrorCodeException : Exception
    {
        public readonly EntityResponse<object> Entity;

        public ErrorCodeException(ErrorCode code, object? data = null) : base(GetMessage(code))
        {
            Code = code;
            Data = data;
            Entity = ToJson1(Code, Data);
        }

        private new object? Data { get; }
        private ErrorCode Code { get; }

        public EntityResponse<object> GetJson()
        {
            return Entity;
        }
    }
}