// TaksiApp.Shared.Kernel/Results/Result.cs
namespace TaksiApp.Shared.Kernel.Results;

public readonly struct Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on success result")
        : _error!.Value;

    private Result(bool isSuccess, Error? error = null)
    {
        if (isSuccess && error.HasValue)
            throw new ArgumentException("Success result cannot have error");
        if (!isSuccess && !error.HasValue)
            throw new ArgumentException("Failure result must have error");

        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    // Pattern matching
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error);
    }
    public Result Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    public async Task<Result> TapAsync(Func<Task> action)
    {
        if (IsSuccess)
            await action().ConfigureAwait(false);
        return this;
    }
    // Ensure
    public Result Ensure(Func<bool> predicate, Error error)
    {
        if (IsFailure)
            return this;

        return predicate() ? Success() : Failure(error);
    }
}
