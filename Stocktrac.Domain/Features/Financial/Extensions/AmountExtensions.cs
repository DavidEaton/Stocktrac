using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// Pure arithmetic operations for <see cref="Amount"/> values.
/// </summary>
public static class AmountExtensions
{
    public static Result<Amount> Add(this Amount amount, Amount other) =>
        Calculate(amount.Value, other.Value, static (left, right) => left + right);

    public static Result<Amount> Subtract(this Amount amount, Amount other) =>
        Calculate(amount.Value, other.Value, static (left, right) => left - right);

    public static Result<Amount> Multiply(this Amount amount, decimal multiplier) =>
        Calculate(amount.Value, multiplier, static (value, factor) => value * factor);

    public static Result<Amount> Negate(this Amount amount) =>
        Calculate(amount.Value, static value => -value);

    private static Result<Amount> Calculate(
        decimal left,
        decimal right,
        Func<decimal, decimal, decimal> calculation)
    {
        try
        {
            return Result.Success(Amount.FromDecimal(calculation(left, right)));
        }
        catch (OverflowException)
        {
            return Result.Failure<Amount>(Amount.OverflowMessage);
        }
    }

    private static Result<Amount> Calculate(
        decimal value,
        Func<decimal, decimal> calculation)
    {
        try
        {
            return Result.Success(Amount.FromDecimal(calculation(value)));
        }
        catch (OverflowException)
        {
            return Result.Failure<Amount>(Amount.OverflowMessage);
        }
    }
}
