namespace TaksiApp.Shared.Kernel.Results;

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
/// <remarks>
/// <para>
/// <see cref="Result{T}"/> models success or failure explicitly,
/// avoiding exception-driven control flow.
/// </para>
/// <para>
/// On success, the result contains a value of type <typeparamref name="T"/>.
/// On failure, it contains an <see cref="Error"/>.
/// </para>
/// </remarks>
public readonly struct Result<T>
{
    private readonly T? _value;
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
    /// Gets the value associated with a successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessing <see cref="Value"/> on a failed result.
    /// </exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on failure result");

    /// <summary>
    /// Gets the error associated with a failed result.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessing <see cref="Error"/> on a successful result.
    /// </exception>
    public Error Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on success result")
        : _error!.Value;

    private Result(bool isSuccess, T? value = default, Error? error = null)
    {
        if (isSuccess && error.HasValue)
            throw new ArgumentException("Success result cannot have error");
        if (!isSuccess && !error.HasValue)
            throw new ArgumentException("Failure result must have error");

        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>
    /// Implicitly converts a value into a successful <see cref="Result{T}"/>.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Executes one of the provided functions depending on the result state
    /// and returns the produced value.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    /// Executes one of the provided actions depending on the result state.
    /// </summary>
    public void Match(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Error);
    }

    /// <summary>
    /// Chains another result-producing operation.
    /// </summary>
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> func) =>
        IsSuccess ? func(Value) : Result<TNext>.Failure(Error);

    /// <summary>
    /// Maps the value of a successful result to a new value.
    /// </summary>
    public Result<TNext> Map<TNext>(Func<T, TNext> func) =>
        IsSuccess ? Result<TNext>.Success(func(Value)) : Result<TNext>.Failure(Error);

    /// <summary>
    /// Executes the specified action if the result is successful.
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(Value);
        return this;
    }

    /// <summary>
    /// Ensures that a condition holds true for the successful value.
    /// </summary>
    /// <param name="predicate">The condition to evaluate.</param>
    /// <param name="error">The error to return if the condition fails.</param>
    /// <returns>
    /// The same result if the condition passes; otherwise, a failure result.
    /// </returns>
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (IsFailure)
            return this;

        return predicate(Value) ? this : Failure(error);
    }
}
