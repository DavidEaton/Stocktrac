using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Financial;

public readonly record struct Fee
{
    private readonly Money _amount;

    public static readonly Money DefaultFee =
        Money.Create(Amount.FromDecimal(0), CurrencyCode.Usd);

    public static Fee Default => new(DefaultFee);

    private Fee(Money amount) =>
        _amount = amount;

    public static Result<Fee> Create(decimal amount, string? currencyCode) =>
        Money.Create(amount, currencyCode).Map(money => new Fee(money));
}
