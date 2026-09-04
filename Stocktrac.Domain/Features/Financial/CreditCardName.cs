using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public readonly record struct CreditCardName
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public const string RequiredMessage = "A valid value is required.";
    public static readonly string InvalidLengthMessage =
        $"Value must be between {MinimumLength} and {MaximumLength} characters.";
    public string Value { get; init; }
    private CreditCardName(string value) =>
        Value = value;

    public static Result<CreditCardName> Create(string? name) =>
        Result.Success(name?.Trim() ?? string.Empty)
            .Ensure(
                value => !string.IsNullOrWhiteSpace(value),
                RequiredMessage)
            .Ensure(
                value => value.Length is >= MinimumLength and <= MaximumLength,
                InvalidLengthMessage)
            .Map(value => new CreditCardName(value));
}
