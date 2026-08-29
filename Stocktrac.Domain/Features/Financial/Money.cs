using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

/// <summary>
/// A monetary amount paired with its currency. All operations return results rather than throwing for expected domain failures.
/// </summary>
public readonly record struct Money
{
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
}
