using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// Pure arithmetic operations for <see cref="Money"/> values.
/// </summary>
public static class MoneyExtensions
{
    public static Result<Money> Add(this Money money, Money other) =>
        Combine(money, other, static (left, right) => left.Add(right));

    public static Result<Money> Subtract(this Money money, Money other) =>
        Combine(money, other, static (left, right) => left.Subtract(right));

    public static Result<Money> Multiply(this Money money, decimal multiplier) =>
        WithCurrency(money.Amount.Multiply(multiplier), money.CurrencyCode);

    public static Result<Money> Negate(this Money money) =>
        WithCurrency(money.Amount.Negate(), money.CurrencyCode);

    private static Result<Money> Combine(
        Money money,
        Money other,
        Func<Amount, Amount, Result<Amount>> operation)
    {
        if (money.CurrencyCode != other.CurrencyCode)
            return Result.Failure<Money>(Money.CurrencyMismatchMessage);

        return WithCurrency(
            operation(money.Amount, other.Amount),
            money.CurrencyCode);
    }

    private static Result<Money> WithCurrency(
        Result<Amount> result,
        CurrencyCode currencyCode) =>
        result.IsFailure
            ? Result.Failure<Money>(result.Error)
            : Result.Success(Money.Create(result.Value, currencyCode));
}
