using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TaksiApp.Shared.Kernel.Guards;

/// <summary>
/// Provides guard clause methods for validating method arguments and preconditions.
/// Throws exceptions when validation fails.
/// </summary>
/// <remarks>
/// Guard clauses are defensive programming techniques that validate inputs at the
/// beginning of methods. This class provides common validation patterns.
/// 
/// Usage:
/// <code>
/// Guard.NotNull(customer, nameof(customer));
/// Guard.Require&lt;ArgumentException&gt;(amount > 0, "Amount must be positive");
/// Guard.InRange(age, 0, 120, nameof(age));
/// </code>
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Throws <typeparamref name="TException"/> if the condition is FALSE.
    /// </summary>
    /// <typeparam name="TException">Type of exception to throw. Must have a constructor accepting (string message).</typeparam>
    /// <param name="condition">The condition to validate. Exception is thrown if FALSE.</param>
    /// <param name="message">The exception message.</param>
    /// <exception cref="TException">Thrown when condition is false.</exception>
    /// <exception cref="InvalidOperationException">Thrown if TException cannot be instantiated.</exception>
    /// <example>
    /// <code>
    /// Guard.Require&lt;ArgumentException&gt;(value >= 0, "Value cannot be negative");
    /// Guard.Require&lt;InvalidOperationException&gt;(isInitialized, "Object not initialized");
    /// </code>
    /// </example>
    public static void Require<TException>(bool condition, string message)
        where TException : Exception
    {
        if (!condition)
        {
            var exception = Activator.CreateInstance(typeof(TException), message) as TException;
            if (exception == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create exception of type {typeof(TException).Name}. " +
                    $"Ensure it has a constructor accepting (string message).");
            }
            throw exception;
        }
    }

    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The reference type being validated.</typeparam>
    /// <param name="value">The value to check for null.</param>
    /// <param name="paramName">The name of the parameter (automatically captured via CallerArgumentExpression).</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <example>
    /// <code>
    /// Guard.NotNull(customer); // paramName is automatically "customer"
    /// Guard.NotNull(order, "customOrderName"); // Override parameter name
    /// </code>
    /// </example>
    public static void NotNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> if the string is null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter (automatically captured via CallerArgumentExpression).</param>
    /// <exception cref="ArgumentException">Thrown when value is null, empty, or whitespace.</exception>
    /// <example>
    /// <code>
    /// Guard.NotNullOrEmpty(name); // Validates name is not null/empty/whitespace
    /// </code>
    /// </example>
    public static void NotNullOrEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null, empty, or whitespace", paramName);
        }
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if value is not greater than the minimum.
    /// </summary>
    /// <typeparam name="T">The type of value being compared (must implement IComparable).</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The exclusive minimum value.</param>
    /// <param name="paramName">The name of the parameter (automatically captured via CallerArgumentExpression).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value &lt;= min.</exception>
    /// <example>
    /// <code>
    /// Guard.GreaterThan(age, 0); // age must be > 0
    /// Guard.GreaterThan(price, 0.0m); // price must be > 0
    /// </code>
    /// </example>
    public static void GreaterThan<T>(
        T value,
        T min,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) <= 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Value must be greater than {min}");
        }
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if value is not greater than or equal to the minimum.
    /// </summary>
    /// <typeparam name="T">The type of value being compared (must implement IComparable).</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The inclusive minimum value.</param>
    /// <param name="paramName">The name of the parameter (automatically captured via CallerArgumentExpression).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value &lt; min.</exception>
    /// <example>
    /// <code>
    /// Guard.GreaterThanOrEqual(count, 0); // count must be >= 0
    /// </code>
    /// </example>
    public static void GreaterThanOrEqual<T>(
        T value,
        T min,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Value must be greater than or equal to {min}");
        }
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if value is outside the specified range (inclusive).
    /// </summary>
    /// <typeparam name="T">The type of value being compared (must implement IComparable).</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The inclusive minimum value.</param>
    /// <param name="max">The inclusive maximum value.</param>
    /// <param name="paramName">The name of the parameter (automatically captured via CallerArgumentExpression).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value &lt; min or value &gt; max.</exception>
    /// <example>
    /// <code>
    /// Guard.InRange(age, 0, 120); // age must be between 0 and 120 inclusive
    /// Guard.InRange(percentage, 0.0, 100.0); // percentage must be between 0 and 100
    /// </code>
    /// </example>
    public static void InRange<T>(
        T value,
        T min,
        T max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Value must be between {min} and {max} (inclusive)");
        }
    }
}