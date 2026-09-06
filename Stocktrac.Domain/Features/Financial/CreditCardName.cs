using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public sealed record CreditCardName
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public const string RequiredMessage = "A valid value is required.";
    public static readonly string InvalidLengthMessage =
        $"Value must be between {MinimumLength} and {MaximumLength} characters.";
    public string Value { get; }
    private CreditCardName(string value) =>
        Value = value;

    public static Result<CreditCardName> Create(string? name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(string.IsNullOrWhiteSpace(normalized), RequiredMessage),
                Result.FailureIf(!string.IsNullOrWhiteSpace(normalized) && normalized.Length is < MinimumLength or > MaximumLength, InvalidLengthMessage))
            .Map(() => new CreditCardName(normalized));
    }
}
