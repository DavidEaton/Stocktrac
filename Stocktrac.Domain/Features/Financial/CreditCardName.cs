using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public sealed record CreditCardName
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MinimumLength = 1;
    public const int MaximumLength = 255;
    public static readonly string InvalidLengthMessage =
        $"Value must be between {MinimumLength} and {MaximumLength} characters.";
    public NonEmptyString Value { get; }
    private CreditCardName(NonEmptyString value) =>
        Value = value;

    public static Result<CreditCardName> Create(NonEmptyString name) =>
        name.Value.Length is >= MinimumLength and <= MaximumLength
            ? Result.Success(new CreditCardName(name))
            : Result.Failure<CreditCardName>(InvalidLengthMessage);

    public override string ToString() => Value.ToString();
}
