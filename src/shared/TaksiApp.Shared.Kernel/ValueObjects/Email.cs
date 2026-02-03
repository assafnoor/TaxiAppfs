using System.Text.RegularExpressions;
using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a validated email address as a ValueObject.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Create email from string with validation.
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>Success with Email or Failure with validation error</returns>
    /// <remarks>
    /// Validation rules:
    /// - Email must not be null or whitespace
    /// - Email must match basic format: local-part@domain.tld
    /// - Email is normalized to lowercase
    /// 
    /// NOTE: This uses a basic regex pattern for format validation.
    /// The regex pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
    /// 
    /// Limitations:
    /// - Does not validate against RFC 5322 fully
    /// - Does not check if domain exists or is valid
    /// - Does not validate local-part length limits (64 chars) or domain length limits (255 chars)
    /// - Does not handle quoted strings or comments in email addresses
    /// - Does not validate internationalized domain names (IDN)
    /// 
    /// For production use requiring strict validation, consider:
    /// - Using a library like MailKit's MimeKit for RFC-compliant validation
    /// - Performing domain MX record lookup for existence verification
    /// - Implementing length checks (local-part: max 64 chars, domain: max 255 chars)
    /// </remarks>
    public static Result<Email> Create(string? email)
    {
        var trimmed = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmed))
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.Empty",
                    "Email address is required"));

        // Length validation (RFC 5321: local-part max 64, @ symbol, domain max 255 = total max 320)
        if (trimmed.Length > 320)
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.TooLong",
                    $"Email address exceeds maximum length of 320 characters: {trimmed.Length} characters"));

        // Basic format validation
        if (!EmailRegex.IsMatch(trimmed))
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.InvalidFormat",
                    $"Email address '{trimmed}' has invalid format. Expected format: local-part@domain.tld"));

        // Additional validation: ensure @ appears only once
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex >= trimmed.Length - 1)
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.InvalidFormat",
                    $"Email address '{trimmed}' has invalid format. @ symbol must be between local-part and domain"));

        // Validate local-part and domain are not empty
        var localPart = trimmed.Substring(0, atIndex);
        var domain = trimmed.Substring(atIndex + 1);

        if (string.IsNullOrWhiteSpace(localPart))
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.EmptyLocalPart",
                    "Email address local-part cannot be empty"));

        if (string.IsNullOrWhiteSpace(domain))
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.EmptyDomain",
                    "Email address domain cannot be empty"));

        // Validate domain contains at least one dot (for TLD)
        if (!domain.Contains('.'))
            return Result.Failure<Email>(
                Error.Validation(
                    "Email.InvalidDomain",
                    $"Email address domain '{domain}' must contain at least one dot (TLD required)"));

        return Result.Success(new Email(trimmed.ToLowerInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Implicit conversion for convenience
    public static implicit operator string(Email email) => email.Value;
}
