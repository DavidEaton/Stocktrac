namespace Stocktrac.Domain.Features.Financial;

public readonly record struct Fee
{
    private readonly Money _amount;

    public static readonly Money DefaultFee =
        Money.Create(Amount.FromDecimal(0), CurrencyCode.Usd);

    public static Fee Default => new(DefaultFee);

    private Fee(Money amount) =>
        _amount = amount;

    public static Fee Create(decimal amount, string? currencyCode)
    {
        var moneyResult = Money.Create(amount, currencyCode);

        return moneyResult.IsFailure
            ? throw new ArgumentException(moneyResult.Error, nameof(amount))
            : new Fee(moneyResult.Value);
    }
}