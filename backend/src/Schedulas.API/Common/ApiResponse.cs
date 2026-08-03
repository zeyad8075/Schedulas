namespace Schedulas.API.Common;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<FieldError>? Errors { get; init; }
    public ApiMeta Meta { get; init; } = new();

    public static ApiResponse<T> Ok(T data, string message = "تمت العملية بنجاح") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<FieldError>? errors = null) =>
        new() { Success = false, Data = default, Message = message, Errors = errors };
}

public sealed record FieldError(string Field, string Message);

public sealed class ApiMeta
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string TraceId { get; init; } = System.Diagnostics.Activity.Current?.Id
        ?? Guid.NewGuid().ToString();
}
