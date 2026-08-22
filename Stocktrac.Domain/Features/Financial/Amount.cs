using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// The signed decimal quantity of a monetary value, independent of currency.
/// </summary>
/// <remarks>
/// Every <see cref="decimal"/> is a valid amount. Negative values represent outflows, debts, or
/// refunds, while zero and positive values represent the corresponding neutral and positive
/// quantities. Currency-specific rules such as minor-unit precision and transaction limits belong
/// to the policy governing a <see cref="Money"/> transaction, not to this currency-independent type.
/// </remarks>
public readonly record struct Amount
{
    public const string OverflowMessage = "The monetary amount is outside the supported range.";

    public decimal Value { get; }

    private Amount(decimal value) =>
        Value = value;

    public static Amount FromDecimal(decimal value) =>
        new(value);

    public Result<Amount> Add(Amount other) =>
        Calculate(Value, other.Value, static (left, right) => left + right);

    public Result<Amount> Subtract(Amount other) =>
        Calculate(Value, other.Value, static (left, right) => left - right);

    public Result<Amount> Multiply(decimal multiplier) =>
        Calculate(Value, multiplier, static (amount, factor) => amount * factor);

    public Result<Amount> Negate() =>
        Calculate(Value, static amount => -amount);

    private static Result<Amount> Calculate(
        decimal left,
        decimal right,
        Func<decimal, decimal, decimal> calculation)
    {
        try
        {
            return Result.Success(FromDecimal(calculation(left, right)));
        }
        catch (OverflowException)
        {
            return Result.Failure<Amount>(OverflowMessage);
        }
    }

    private static Result<Amount> Calculate(decimal value, Func<decimal, decimal> calculation)
    {
        try
        {
            return Result.Success(FromDecimal(calculation(value)));
        }
        catch (OverflowException)
        {
            return Result.Failure<Amount>(OverflowMessage);
        }
    }

    public override string ToString() =>
        Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}