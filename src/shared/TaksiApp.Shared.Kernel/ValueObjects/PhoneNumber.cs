// TaksiApp.Shared.Kernel/ValueObjects/PhoneNumber.cs
using System.Text.RegularExpressions;
using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Kernel.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex CountryCodeRegex = new(@"^\d{1,3}$", RegexOptions.Compiled);
    private static readonly Regex NumberRegex = new(@"^\d{6,15}$", RegexOptions.Compiled);

    public string CountryCode { get; }
    public string Number { get; }
    public string FullNumber => $"+{CountryCode}{Number}";

    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number = number;
    }

    /// <summary>
    /// Creates a PhoneNumber value object with validation.
    /// </summary>
    /// <param name="countryCode">Country calling code (1-3 digits, e.g., "1" for US, "44" for UK)</param>
    /// <param name="number">Phone number without country code (6-15 digits)</param>
    /// <returns>Success with PhoneNumber or Failure with validation error</returns>
    /// <remarks>
    /// Validation rules:
    /// - Country code: 1-3 digits (ITU-T E.164 format)
    /// - Phone number: 6-15 digits (ITU-T E.164 format)
    /// - Both parts are required and cannot be empty
    /// 
    /// NOTE: This uses basic format validation only.
    /// 
    /// Limitations:
    /// - Does not validate against actual country code registry (ITU-T E.164)
    /// - Does not verify if country code and number combination is valid for that country
    /// - Does not handle extensions or special formatting
    /// - Does not validate against country-specific number length rules
    /// - Does not handle internationalized phone numbers (non-digit characters)
    /// 
    /// For production use requiring strict validation, consider:
    /// - Using a library like libphonenumber (Google's phone number library)
    /// - Validating against ITU-T E.164 country code registry
    /// - Implementing country-specific validation rules
    /// - Supporting internationalized phone numbers if needed
    /// </remarks>
    public static Result<PhoneNumber> Create(string? countryCode, string? number)
    {
        var cc = countryCode?.Trim() ?? string.Empty;
        var num = number?.Trim() ?? string.Empty;

        // Validate country code presence
        if (string.IsNullOrWhiteSpace(cc))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.CountryCodeEmpty", "Country code is required"));

        // Validate phone number presence
        if (string.IsNullOrWhiteSpace(num))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.NumberEmpty", "Phone number is required"));

        // Validate country code format (1-3 digits, ITU-T E.164)
        if (!CountryCodeRegex.IsMatch(cc))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.InvalidCountryCode",
                    $"Country code '{cc}' must be 1-3 digits (ITU-T E.164 format)"));

        // Validate phone number format (6-15 digits, ITU-T E.164)
        if (!NumberRegex.IsMatch(num))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.InvalidNumber",
                    $"Phone number '{num}' must be 6-15 digits (ITU-T E.164 format)"));

        // Additional validation: country code should not start with 0
        if (cc.StartsWith("0"))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.InvalidCountryCode",
                    $"Country code '{cc}' cannot start with 0"));

        // Additional validation: total length should not exceed 15 digits (E.164 max)
        var totalLength = cc.Length + num.Length;
        if (totalLength > 15)
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.TooLong",
                    $"Total phone number length ({totalLength} digits) exceeds maximum of 15 digits (ITU-T E.164)"));

        return Result.Success(new PhoneNumber(cc, num));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }

    public override string ToString() => FullNumber;
}

