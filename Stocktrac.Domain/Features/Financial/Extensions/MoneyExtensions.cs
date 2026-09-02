namespace Stocktrac.Domain.Features.Financial.Extensions;

/// <summary>
/// Presentation helpers for <see cref="Money"/> values.
/// </summary>
public static class MoneyExtensions
{
    public static string ToDisplayString(this Money money) =>
        $"{money.Amount} {money.CurrencyCode}";
}
