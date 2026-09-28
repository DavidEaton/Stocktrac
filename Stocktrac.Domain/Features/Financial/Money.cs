using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public readonly record struct Money
{
    public const string CurrencyMismatchMessage = "Money values must have the same currency.";

    public Amount Amount { get; }
    public CurrencyCode CurrencyCode { get; }

    private Money(Amount amount, CurrencyCode currencyCode) =>
        (Amount, CurrencyCode) = (amount, currencyCode);

    public static Money Create(Amount amount, CurrencyCode currencyCode) =>
        new(amount, currencyCode);

    public static Result<Money> Create(decimal amount, string currencyCode) =>
        NonEmptyString.Create(currencyCode)
            .Bind(CurrencyCode.Create)
            .Map(
            code => Create(Amount.FromDecimal(amount), code));

    public Result<Money> Add(Money other) =>
        Combine(other, static (left, right) => left.Add(right));

    public Result<Money> Subtract(Money other) =>
        Combine(other, static (left, right) => left.Subtract(right));

    public Result<Money> Multiply(decimal multiplier) =>
        ReplaceCurrency(Amount.Multiply(multiplier));

    public Result<Money> Negate() =>
        ReplaceCurrency(Amount.Negate());

    private Result<Money> Combine(
        Money other,
        Func<Amount, Amount, Result<Amount>> operation)
    {
        if (CurrencyCode != other.CurrencyCode)
            return Result.Failure<Money>(CurrencyMismatchMessage);

        return ReplaceCurrency(operation(Amount, other.Amount));
    }

    private Result<Money> ReplaceCurrency(Result<Amount> result)
    {
        var currencyCode = CurrencyCode;

        return result.Map(
            amount => Create(amount, currencyCode));
    }
}
