namespace StockOrders.Application.Common;

public enum ErrorType
{
    NotFound,
    Conflict,
    Validation,
    Failure
}

public record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failure result.");

    private Result(T value) : base(true, Error.None) => _value = value;
    private Result(Error error) : base(false, error) { }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
