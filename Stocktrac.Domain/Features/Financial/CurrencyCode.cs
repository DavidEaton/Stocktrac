using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// An ISO 4217-style alphabetic currency code.
/// </summary>
public readonly record struct CurrencyCode
{
    public const int CodeLength = 3;
    public const string DefaultCode = "USD";
    public const string RequiredMessage = "Currency code is required.";
    public const string InvalidMessage =
        "Currency code must be three alphabetic characters.";
    public const string UnsupportedMessage =
        "Currency code is not an active ISO 4217 code.";

    // null internally means DefaultCode.
    private readonly string? _nonDefaultCode;

    public static CurrencyCode Usd => new("USD");

    public static CurrencyCode Default => new(DefaultCode);

    public string Value =>
        _nonDefaultCode ?? DefaultCode;

    private CurrencyCode(string code) =>
        _nonDefaultCode = code == DefaultCode ? null : code;

    public static Result<CurrencyCode> Create(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<CurrencyCode>(RequiredMessage);

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != CodeLength ||
            normalizedCode.Any(character => !char.IsAsciiLetter(character)))
        {
            return Result.Failure<CurrencyCode>(InvalidMessage);
        }

        if (!Iso4217CurrencyCodes.Contains(normalizedCode))
            return Result.Failure<CurrencyCode>(UnsupportedMessage);

        return Result.Success(new CurrencyCode(normalizedCode));
    }

    public override string ToString() =>
        Value;
}