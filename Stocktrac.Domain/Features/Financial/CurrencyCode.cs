using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// An ISO 4217-style alphabetic currency code.
/// </summary>
public readonly record struct CurrencyCode
{
    public const int CodeLength = 3;
    public const string DefaultCode = "USD";
    public const string InvalidMessage = "Currency code must be three alphabetic characters.";
    public const string UnsupportedMessage = "Currency code is not an active ISO 4217 code.";

    // null internally means DefaultCode.
    private readonly string? _nonDefaultCode;

    public static CurrencyCode Usd => new("USD");

    public static CurrencyCode Default => new(DefaultCode);

    public string Value =>
        _nonDefaultCode ?? DefaultCode;

    private CurrencyCode(string code) =>
        _nonDefaultCode = code == DefaultCode ? null : code;

    public static Result<CurrencyCode> Create(NonEmptyString code) =>
        Result.Success(code)
            .Ensure(value => value.Value.Length == CodeLength, InvalidMessage)
            .Ensure(value => value.Value.All(char.IsAsciiLetter), InvalidMessage)
            .Ensure(value => Iso4217CountryCurrencyCodes.Contains(NormalizeCode(value.Value)), UnsupportedMessage)
            .Map(value => new CurrencyCode(NormalizeCode(value.Value)));

    private static string NormalizeCode(string code) => code.ToUpperInvariant();

    public override string ToString() =>
        Value;
}
