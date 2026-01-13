using WPFLauncherApi.Entities;

namespace WPFLauncherApi.Utils.CodeTools;

public class ErrorCodeException(ErrorCode errorCode, object? data = null) : Exception(Code.GetMessage(errorCode))
{
    public readonly EntityResponse<object> Entity = Code.ToJson1(errorCode, data);

    public ErrorCodeException() : this(ErrorCode.Failure)
    {
    }

    // private new object? Data { get; } = data;
    // private ErrorCode ErrorCode { get; } = errorCode;

    public EntityResponse<object> GetJson()
    {
        return Entity;
    }
}