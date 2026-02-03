
// TaksiApp.Shared.Kernel/Results/Result{T}.cs
namespace TaksiApp.Shared.Kernel.Results;

public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on failure result");

    public Error Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on success result")
        : _error!.Value;

    private Result(bool isSuccess, T? value = default, Error? error = null)
    {
        if (isSuccess && error.HasValue)
            throw new ArgumentException("Success result cannot have error");
        if (!isSuccess && !error.HasValue)
            throw new ArgumentException("Failure result must have error");
        
        // Note: We don't check for null here because:
        // 1. For nullable types (T?), null is a valid value
        // 2. For non-nullable reference types, the compiler will warn
        // 3. For value types, null is not possible
        // If null is passed for a non-nullable type, it's a programming error
        // that should be caught at compile time with nullable reference types enabled

        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(Error error) => new(false, default, error);

    // Implicit conversion from T
    public static implicit operator Result<T>(T value) => Success(value);

    // Pattern matching
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    public void Match(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Error);
    }

    // Monadic operations
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> func) =>
        IsSuccess ? func(Value) : Result<TNext>.Failure(Error);

    public Result<TNext> Map<TNext>(Func<T, TNext> func) =>
        IsSuccess ? Result<TNext>.Success(func(Value)) : Result<TNext>.Failure(Error);

    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(Value);
        return this;
    }

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (IsFailure)
            return this;

        return predicate(Value) ? this : Failure(error);
    }
}