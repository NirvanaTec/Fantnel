namespace OpenSDK.Entities;

public class Result(bool isSuccess, string? error = null) {
    public bool IsSuccess { get; } = isSuccess;
    public bool IsFailure => !IsSuccess;
    public string? Error { get; } = error;

    public static Result Success()
    {
        return new Result(true);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    public static Result Clone(Result result)
    {
        return new Result(result.IsSuccess, result.Error);
    }
}

public class Result<T> : Result {
    private Result(T value) : base(true)
    {
        Value = value;
    }

    private Result(string error) : base(false, error)
    {
        Value = default;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public new static Result<T> Failure(string error)
    {
        return new Result<T>(error);
    }
}