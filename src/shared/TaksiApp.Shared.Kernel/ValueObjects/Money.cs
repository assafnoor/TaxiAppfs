// TaksiApp.Shared.Kernel/ValueObjects/Money.cs
using System.Linq;
using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Kernel.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a Money value object with validation.
    /// </summary>
    /// <param name="amount">Monetary amount (must be non-negative)</param>
    /// <param name="currency">ISO 4217 currency code (3 uppercase letters, e.g., "USD", "EUR")</param>
    /// <returns>Success with Money or Failure with validation error</returns>
    /// <remarks>
    /// Validation rules:
    /// - Amount must be non-negative
    /// - Currency code must be exactly 3 characters (ISO 4217 format)
    /// - Currency code is converted to uppercase
    /// 
    /// NOTE: This method validates format only. It does not verify against
    /// the official ISO 4217 currency list. Invalid currency codes (e.g., "XXX")
    /// will be accepted if they meet the format requirements.
    /// For production use, consider validating against a known currency list.
    /// </remarks>
    public static Result<Money> Create(decimal amount, string? currency)
    {
        var curr = currency?.Trim().ToUpperInvariant() ?? string.Empty;

        // Validate amount
        if (amount < 0)
            return Result.Failure<Money>(
                Error.Validation("Money.NegativeAmount",
                    $"Amount cannot be negative: {amount}"));

        // Check for overflow (decimal max value is very large, but document the limit)
        if (amount > 999_999_999_999_999.99m)
            return Result.Failure<Money>(
                Error.Validation("Money.AmountTooLarge",
                    $"Amount exceeds maximum allowed value: {amount}"));

        // Validate currency code presence
        if (string.IsNullOrWhiteSpace(curr))
            return Result.Failure<Money>(
                Error.Validation("Money.EmptyCurrency", "Currency code is required"));

        // Validate currency code format (ISO 4217: exactly 3 uppercase letters)
        if (curr.Length != 3)
            return Result.Failure<Money>(
                Error.Validation("Money.InvalidCurrencyLength",
                    $"Currency code '{curr}' must be exactly 3 characters (ISO 4217 format)"));

        // Validate currency code contains only letters
        if (!System.Linq.Enumerable.All(curr, char.IsLetter))
            return Result.Failure<Money>(
                Error.Validation("Money.InvalidCurrencyFormat",
                    $"Currency code '{curr}' must contain only letters (ISO 4217 format)"));

        return Result.Success(new Money(amount, curr));
    }

    // Arithmetic operators
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot add money in different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot subtract money in different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}