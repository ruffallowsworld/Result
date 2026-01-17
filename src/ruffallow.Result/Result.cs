using System;

namespace ruffallow.Result;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value of type <typeparamref name="T"/>
/// or fail with an error of type <typeparamref name="TE"/>.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
/// <typeparam name="TE">Type of the failure value.</typeparam>
public sealed class Result<T, TE>
{
    /// <summary>
    /// Gets the successful value if the result represents success; otherwise <c>null</c>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the failure value if the result represents failure; otherwise <c>null</c>.
    /// </summary>
    public TE? Error { get; }

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the result represents a failed operation.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = default;
    }

    private Result(TE error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> into a successful result.
    /// </summary>
    public static implicit operator Result<T, TE>(T value) => new(value);

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="TE"/> into a failed result.
    /// </summary>
    public static implicit operator Result<T, TE>(TE error) => new(error);

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<T, TE> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    public static Result<T, TE> Failure(TE error) => new(error);

    /// <summary>
    /// Matches the result and executes one of the provided functions,
    /// returning a new <see cref="Result{T,E}"/>.
    /// </summary>
    /// <param name="onSuccess">Function to apply when the result is successful.</param>
    /// <param name="onFailure">Function to apply when the result is failed.</param>
    public Result<T, TE> Match(
        Func<T, Result<T, TE>> onSuccess,
        Func<TE, Result<T, TE>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(Error!);
    }

    /// <summary>
    /// Matches the result and returns a value of type <typeparamref name="T"/>.
    /// On failure, the provided function is used to create and throw an exception.
    /// </summary>
    /// <param name="onSuccess">Function to transform the success value.</param>
    /// <param name="onFailure">
    /// Function that receives the error and returns an exception to be thrown.
    /// </param>
    public T Match(
        Func<T, T> onSuccess,
        Func<TE, Exception> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
        {
            return onSuccess(Value!);
        }

        throw onFailure(Error!);
    }

    /// <summary>
    /// Returns the success value or throws if the result is a failure.
    /// </summary>
    public T Unwrap()
    {
        if (IsSuccess)
        {
            return Value!;
        }

        throw new InvalidOperationException($"Unwrap failed. Result is a failure with error: {Error}");
    }

    /// <summary>
    /// Returns the success value or the provided default value if the result is a failure.
    /// </summary>
    /// <param name="defaultValue">Value to return when the result is a failure.</param>
    public T UnwrapOrDefault(T defaultValue)
    {
        return IsSuccess ? Value! : defaultValue;
    }

    /// <summary>
    /// Attempts to unwrap the result without throwing.
    /// </summary>
    /// <param name="value">Outputs the success value if available.</param>
    /// <param name="error">Outputs the error if available.</param>
    /// <returns><c>true</c> if the result is successful; otherwise <c>false</c>.</returns>
    public bool TryUnwrap(out T? value, out TE? error)
    {
        if (IsSuccess)
        {
            value = Value;
            error = default;
            return true;
        }

        value = default;
        error = Error;
        return false;
    }

    /// <summary>
    /// Transforms the success value using the given mapping function,
    /// leaving the error unchanged when the result is a failure.
    /// </summary>
    public Result<TResult, TE> Map<TResult>(Func<T, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess
            ? Result<TResult, TE>.Success(mapper(Value!))
            : Result<TResult, TE>.Failure(Error!);
    }

    /// <summary>
    /// Chains another operation that also returns a <see cref="Result{T,E}"/>.
    /// This is also known as "Bind" or "FlatMap".
    /// </summary>
    public Result<TResult, TE> Bind<TResult>(Func<T, Result<TResult, TE>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsSuccess
            ? binder(Value!)
            : Result<TResult, TE>.Failure(Error!);
    }

    /// <summary>
    /// Executes the specified action when the result is successful.
    /// </summary>
    public Result<T, TE> OnSuccess(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsSuccess)
        {
            action(Value!);
        }

        return this;
    }

    /// <summary>
    /// Executes the specified action when the result is a failure.
    /// </summary>
    public Result<T, TE> OnFailure(Action<TE> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsFailure)
        {
            action(Error!);
        }

        return this;
    }
}
