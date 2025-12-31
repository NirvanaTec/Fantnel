using WPFLauncherApi.Entities;

namespace WPFLauncherApi.Utils.CodeTools;

public class ErrorCodeException : Exception
{
    public readonly EntityResponse<object> Entity;

    public ErrorCodeException() : this(ErrorCode.Failure)
    {
    }

    public ErrorCodeException(ErrorCode errorCode, object? data = null) : base(Code.GetMessage(errorCode))
    {
        ErrorCode = errorCode;
        Data = data;
        Entity = Code.ToJson1(errorCode, data);
    }

    private new object? Data { get; }
    private ErrorCode ErrorCode { get; }

    public EntityResponse<object> GetJson()
    {
        return Entity;
    }
}