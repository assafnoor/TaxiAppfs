namespace TaksiApp.Shared.Kernel.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
/// <remarks>
/// <para>
/// This type implements the Result pattern to model success or failure
/// without relying on exceptions for control flow.
/// </para>
/// <para>
/// A successful result contains no data, while a failure result
/// contains an <see cref="Error"/> describing the failure.
/// </para>
/// </remarks>
public readonly struct Result
{
    private readonly Error? _error;

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with a failed result.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessing <see cref="Error"/> on a successful result.
    /// </exception>
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

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with the specified value.
    /// </summary>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with the specified error.
    /// </summary>
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    /// <summary>
    /// Executes one of the provided functions depending on the result state
    /// and returns the produced value.
    /// </summary>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    /// <summary>
    /// Executes one of the provided actions depending on the result state.
    /// </summary>
    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error);
    }

    /// <summary>
    /// Executes the specified action if the result is successful.
    /// </summary>
    public Result Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    /// <summary>
    /// Executes the specified asynchronous action if the result is successful.
    /// </summary>
    public async Task<Result> TapAsync(Func<Task> action)
    {
        if (IsSuccess)
            await action().ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Ensures that a condition holds true for a successful result.
    /// </summary>
    /// <param name="predicate">The condition to evaluate.</param>
    /// <param name="error">The error to return if the condition fails.</param>
    /// <returns>
    /// The same result if the condition passes; otherwise, a failure result.
    /// </returns>
    public Result Ensure(Func<bool> predicate, Error error)
    {
        if (IsFailure)
            return this;

        return predicate() ? Success() : Failure(error);
    }
}
