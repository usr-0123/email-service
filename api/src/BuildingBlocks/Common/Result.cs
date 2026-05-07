namespace EmailPlatform.BuildingBlocks.Common;

public readonly record struct Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    private Result(bool ok, Error error)
    {
        IsSuccess = ok;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    private Result(bool ok, T? value, Error error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);
}
