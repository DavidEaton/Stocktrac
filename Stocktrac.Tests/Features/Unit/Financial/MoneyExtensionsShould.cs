using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class MoneyExtensionsShould
{
    [Theory]
    [InlineData("1234.50", "EUR", "1234.50 EUR")]
    [InlineData("-42.75", "GBP", "-42.75 GBP")]
    [InlineData("0", "JPY", "0 JPY")]
    [InlineData("0.0000000000000000000000000001", "USD", "0.0000000000000000000000000001 USD")]
    [InlineData("79228162514264337593543950335", "AUD", "79228162514264337593543950335 AUD")]
    [InlineData("-79228162514264337593543950335", "CAD", "-79228162514264337593543950335 CAD")]
    public void ReturnAmountFollowedByCurrency_On_ToDisplayString_WhenGivenAnyMoney(
        string amountText,
        string currencyCode,
        string expected)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);
        var money = Money.Create(amount, currencyCode).Value;

        var displayString = money.ToDisplayString();

        displayString.ShouldBe(expected);
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("ar-SA")]
    public void UseInvariantAmountFormatting_On_ToDisplayString_WhenCurrentCultureVaries(
        string cultureName)
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            var money = Money.Create(1234.50m, "EUR").Value;

            money.ToDisplayString().ShouldBe("1234.50 EUR");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ReturnZeroUsd_On_ToDisplayString_WhenMoneyIsDefault()
    {
        var money = default(Money);

        money.ToDisplayString().ShouldBe("0 USD");
    }

    [Fact]
    public void ReturnNormalizedCurrency_On_ToDisplayString_WhenCurrencyInputIsNotNormalized()
    {
        var money = Money.Create(10m, " eur ").Value;

        money.ToDisplayString().ShouldBe("10 EUR");
    }

    [Fact]
    public void ReturnSameValue_On_ToDisplayString_WhenCalledAsStaticMethod()
    {
        var money = Money.Create(19.99m, "USD").Value;

        var displayString = MoneyExtensions.ToDisplayString(money);

        displayString.ShouldBe("19.99 USD");
    }
}