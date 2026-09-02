using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class FeeShould
{
    [Fact]
    public void ContainZeroUsd_On_DefaultFee()
    {
        Fee.DefaultFee.Amount.ShouldBe(Amount.FromDecimal(0m));
        Fee.DefaultFee.CurrencyCode.ShouldBe(CurrencyCode.Usd);
    }

    [Fact]
    public void ReturnDefaultFee_On_Default()
    {
        Fee.Default.ShouldBe(Fee.Create(0m, "USD").Value);
    }

    [Fact]
    public void BeEquivalentToDefault_On_DefaultValue()
    {
        default(Fee).ShouldBe(Fee.Default);
    }

    [Theory]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("79228162514264337593543950335")]
    public void ReturnFee_On_Create_WhenAmountIsAnyDecimal(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var fee = Fee.Create(amount, "CAD").Value;

        fee.ShouldBe(Fee.Create(amount, "CAD").Value);
        fee.ShouldNotBe(Fee.Create(amount == decimal.MaxValue ? amount - 1m : amount + 1m, "CAD").Value);
    }

    [Theory]
    [InlineData("usd")]
    [InlineData(" USD ")]
    [InlineData("UsD")]
    public void NormalizeCurrency_On_Create_WhenCurrencyTextIsValid(string currencyCode)
    {
        var fee = Fee.Create(12.34m, currencyCode).Value;

        fee.ShouldBe(Fee.Create(12.34m, "USD").Value);
    }

    [Theory]
    [InlineData(null, CurrencyCode.InvalidMessage)]
    [InlineData("", CurrencyCode.InvalidMessage)]
    [InlineData("   ", CurrencyCode.InvalidMessage)]
    [InlineData("US", CurrencyCode.InvalidMessage)]
    [InlineData("US1", CurrencyCode.InvalidMessage)]
    [InlineData("USDD", CurrencyCode.InvalidMessage)]
    [InlineData("ZZZ", CurrencyCode.UnsupportedMessage)]
    public void ReturnFailure_On_Create_WhenCurrencyTextIsInvalid(
        string? currencyCode,
        string expectedError)
    {
        var result = Fee.Create(1m, currencyCode);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenAmountAndNormalizedCurrencyAreEqual()
    {
        var first = Fee.Create(2.5m, "EUR").Value;
        var second = Fee.Create(2.5m, " eur ").Value;

        (first == second).ShouldBeTrue();
        (first != second).ShouldBeFalse();
        first.Equals(second).ShouldBeTrue();
        first.Equals((object)second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Theory]
    [InlineData(2.5, 3.5, "USD", "USD")]
    [InlineData(2.5, 2.5, "USD", "EUR")]
    public void NotBeEqual_WhenAmountOrCurrencyDiffers(
        decimal firstAmount,
        decimal secondAmount,
        string firstCurrency,
        string secondCurrency)
    {
        var first = Fee.Create(firstAmount, firstCurrency).Value;
        var second = Fee.Create(secondAmount, secondCurrency).Value;

        (first == second).ShouldBeFalse();
        (first != second).ShouldBeTrue();
        first.Equals(second).ShouldBeFalse();
    }
}
