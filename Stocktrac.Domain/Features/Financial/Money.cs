using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// A monetary amount paired with its currency. All operations return results rather than throwing for expected domain failures.
/// </summary>
public readonly record struct Money
{
    public const string CurrencyMismatchMessage = "Money values must have the same currency.";

    public Amount Amount { get; }
    public CurrencyCode CurrencyCode { get; }

    private Money(Amount amount, CurrencyCode currencyCode)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public static Money Create(Amount amount, CurrencyCode currencyCode) =>
        new(amount, currencyCode);

    public static Result<Money> Create(decimal amount, string? currencyCode) =>
        CurrencyCode.Create(currencyCode).Map(
            code => Create(Amount.FromDecimal(amount), code));

    public Result<Money> Add(Money other) =>
        Combine(other, static (left, right) => left.Add(right));

    public Result<Money> Subtract(Money other) =>
        Combine(other, static (left, right) => left.Subtract(right));

    public Result<Money> Multiply(decimal multiplier) =>
        WithCurrency(Amount.Multiply(multiplier));

    public Result<Money> Negate() =>
        WithCurrency(Amount.Negate());

    private Result<Money> Combine(
        Money other,
        Func<Amount, Amount, Result<Amount>> operation)
    {
        if (CurrencyCode != other.CurrencyCode)
            return Result.Failure<Money>(CurrencyMismatchMessage);

        return WithCurrency(operation(Amount, other.Amount));
    }

    private Result<Money> WithCurrency(Result<Amount> result) =>
        result.Map(amount => Create(amount, CurrencyCode));
}
