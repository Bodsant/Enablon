namespace Ehsms.BuildingBlocks;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    private Result(T? value, bool isSuccess, string? error, int statusCode)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T value) => new(value, true, null, 200);
    public static Result<T> Success(T value, int statusCode) => new(value, true, null, statusCode);
    public static Result<T> Fail(string error, int statusCode = 400) => new(default, false, error, statusCode);
    public static Result<T> NotFound(string error = "Resource not found") => new(default, false, error, 404);
    public static Result<T> Unauthorized(string error = "Unauthorized") => new(default, false, error, 401);
    public static Result<T> Forbidden(string error = "Forbidden") => new(default, false, error, 403);
    public static Result<T> Conflict(string error = "Conflict") => new(default, false, error, 409);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, null, 200);
    public static Result Fail(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result NotFound(string error = "Resource not found") => new(false, error, 404);
}
