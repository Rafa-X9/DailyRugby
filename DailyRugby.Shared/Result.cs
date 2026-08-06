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