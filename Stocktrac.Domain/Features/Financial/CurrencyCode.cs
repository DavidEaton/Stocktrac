using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// An ISO 4217-style alphabetic currency code.
/// </summary>
public readonly record struct CurrencyCode
{
    public const int CodeLength = 3;
    public const string RequiredMessage = "Currency code is required.";
    public const string InvalidMessage = "Currency code must be three alphabetic characters.";

    public string Value { get; }

    private CurrencyCode(string code) =>
        Value = code;

    public static Result<CurrencyCode> Create(string? code)
    {
        var normalizedCode = (code ?? string.Empty).Trim().ToUpperInvariant();

        if (normalizedCode.Length != CodeLength || normalizedCode.Any(character => !char.IsAsciiLetter(character)))
            return Result.Failure<CurrencyCode>(InvalidMessage);

        return Result.Success(new CurrencyCode(normalizedCode));
    }

    public override string ToString() =>
        Value ?? string.Empty;
}