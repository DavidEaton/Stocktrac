using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features;
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
        Fee.Default.ShouldBe(CreateFee(0m, CurrencyCode.Usd));
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
        var amountValue = decimal.Parse(amountText, CultureInfo.InvariantCulture);
        var amount = Amount.FromDecimal(amountValue);

        var fee = Fee.Create(amount, CreateCurrency("CAD")).Value;

        fee.ShouldBe(Fee.Create(amount, CreateCurrency("CAD")).Value);
        var differentAmount = amountValue == decimal.MaxValue ? amountValue - 1m : amountValue + 1m;
        fee.ShouldNotBe(Fee.Create(Amount.FromDecimal(differentAmount), CreateCurrency("CAD")).Value);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenAmountAndCurrencyAreEqual()
    {
        var first = CreateFee(2.5m, CreateCurrency("EUR"));
        var second = CreateFee(2.5m, CreateCurrency("EUR"));

        (first == second).ShouldBeTrue();
        (first != second).ShouldBeFalse();
        first.Equals(second).ShouldBeTrue();
        first.Equals((object)second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Theory]
    [InlineData("2.5", "3.5", "USD", "USD")]
    [InlineData("2.5", "2.5", "USD", "EUR")]
    public void NotBeEqual_WhenAmountOrCurrencyDiffers(
        string firstAmount,
        string secondAmount,
        string firstCurrency,
        string secondCurrency)
    {
        var first = CreateFee(decimal.Parse(firstAmount, CultureInfo.InvariantCulture), CreateCurrency(firstCurrency));
        var second = CreateFee(decimal.Parse(secondAmount, CultureInfo.InvariantCulture), CreateCurrency(secondCurrency));

        (first == second).ShouldBeFalse();
        (first != second).ShouldBeTrue();
        first.Equals(second).ShouldBeFalse();
    }

    private static Fee CreateFee(decimal amount, CurrencyCode currencyCode) =>
        Fee.Create(Amount.FromDecimal(amount), currencyCode).Value;

    private static CurrencyCode CreateCurrency(string currencyCode) =>
        CurrencyCode.Create(NonEmptyString.Create(currencyCode).Value).Value;
}
