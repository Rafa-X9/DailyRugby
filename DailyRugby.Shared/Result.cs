using System.Diagnostics.CodeAnalysis;

namespace DailyRugby.Shared;

public sealed class Result
{
    public bool IsSuccessful { get; }
    public string Message { get; }
    public Errors Error { get; }

    private Result(bool isSuccessful, string message, Errors error)
    {
        IsSuccessful = isSuccessful;
        Message = message;
        Error = error;
    }

    public static Result Success()
        => new(true, "Success", Errors.None);

    public static Result Failure(string message, Errors error)
        => new(false, message, error);
}

public sealed class Result<T>
{
    [MemberNotNullWhen(true, nameof(Item))]
    public bool IsSuccessful { get; }
    public T? Item { get; }
    public string Message { get; }
    public Errors Error { get; }

    private Result(bool isSuccessful, T? item, string message, Errors error)
    {
        IsSuccessful = isSuccessful;
        Item = item;
        Message = message;
        Error = error;
    }

    public static Result<T> Success(T item)
        => new(true, item, "Success", Errors.None);

    public static Result<T> Failure(string message, Errors error)
        => new(false, default, message, error);
}