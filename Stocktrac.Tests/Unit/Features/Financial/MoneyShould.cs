using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class MoneyShould
{
    [Fact]
    public void Create_WhenGivenAmountAndCurrencyCode_SetsBothProperties()
    {
        var amount = Amount.FromDecimal(12.34m);
        var currencyCode = CurrencyCode.Create("CAD").Value;

        var money = Money.Create(amount, currencyCode);

        money.Amount.ShouldBe(amount);
        money.CurrencyCode.ShouldBe(currencyCode);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData(" EUR ", "EUR")]
    [InlineData("XAU", "XAU")]
    public void Create_WhenGivenValidCurrencyText_ReturnsMoneyWithNormalizedCurrency(
        string currencyCode,
        string expectedCurrencyCode)
    {
        var result = Money.Create(12.34m, currencyCode);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(Amount.FromDecimal(12.34m));
        result.Value.CurrencyCode.Value.ShouldBe(expectedCurrencyCode);
    }

    [Theory]
    [InlineData(null, CurrencyCode.InvalidMessage)]
    [InlineData("", CurrencyCode.InvalidMessage)]
    [InlineData("   ", CurrencyCode.InvalidMessage)]
    [InlineData("US", CurrencyCode.InvalidMessage)]
    [InlineData("US1", CurrencyCode.InvalidMessage)]
    [InlineData("USDD", CurrencyCode.InvalidMessage)]
    [InlineData("ZZZ", CurrencyCode.UnsupportedMessage)]
    public void Create_WhenGivenInvalidCurrencyText_ReturnsCurrencyFailure(
        string? currencyCode,
        string expectedError)
    {
        var result = Money.Create(1m, currencyCode);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("79228162514264337593543950335")]
    public void Create_WhenGivenAnyDecimalAmount_ReturnsMoney(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var result = Money.Create(amount, "USD");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.Value.ShouldBe(amount);
    }

    [Fact]
    public void Default_WhenUsed_ReturnsZeroUsd()
    {
        var money = default(Money);

        money.Amount.ShouldBe(Amount.FromDecimal(0m));
        money.CurrencyCode.ShouldBe(CurrencyCode.Usd);
    }

    [Fact]
    public void Equality_WhenAmountAndNormalizedCurrencyAreEqual_ReturnsTrue()
    {
        var first = Money.Create(10m, "USD").Value;
        var second = Money.Create(10m, " usd ").Value;

        (first == second).ShouldBeTrue();
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Theory]
    [InlineData(10, 11, "USD", "USD")]
    [InlineData(10, 10, "USD", "EUR")]
    public void Equality_WhenAmountOrCurrencyDiffers_ReturnsFalse(
        decimal firstAmount,
        decimal secondAmount,
        string firstCurrency,
        string secondCurrency)
    {
        var first = Money.Create(firstAmount, firstCurrency).Value;
        var second = Money.Create(secondAmount, secondCurrency).Value;

        (first == second).ShouldBeFalse();
        (first != second).ShouldBeTrue();
    }

    [Theory]
    [InlineData(10.25, 2.75, 13)]
    [InlineData(-10, 3, -7)]
    [InlineData(10, -3, 7)]
    [InlineData(0, 0, 0)]
    public void Add_WhenCurrenciesMatch_ReturnsSumAndPreservesCurrency(
        decimal leftAmount,
        decimal rightAmount,
        decimal expectedAmount)
    {
        var left = Money.Create(leftAmount, "CAD").Value;
        var right = Money.Create(rightAmount, "CAD").Value;

        var result = left.Add(right);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Money.Create(expectedAmount, "CAD").Value);
    }

    [Theory]
    [InlineData(10.25, 2.75, 7.5)]
    [InlineData(-10, 3, -13)]
    [InlineData(10, -3, 13)]
    [InlineData(0, 0, 0)]
    public void Subtract_WhenCurrenciesMatch_ReturnsDifferenceAndPreservesCurrency(
        decimal leftAmount,
        decimal rightAmount,
        decimal expectedAmount)
    {
        var left = Money.Create(leftAmount, "GBP").Value;
        var right = Money.Create(rightAmount, "GBP").Value;

        var result = left.Subtract(right);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Money.Create(expectedAmount, "GBP").Value);
    }

    [Fact]
    public void Add_WhenCurrenciesDiffer_ReturnsCurrencyMismatchFailure()
    {
        var dollars = Money.Create(10m, "USD").Value;
        var euros = Money.Create(10m, "EUR").Value;

        var result = dollars.Add(euros);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MoneyExtensions.CurrencyMismatchMessage);
    }

    [Fact]
    public void Subtract_WhenCurrenciesDiffer_ReturnsCurrencyMismatchFailure()
    {
        var dollars = Money.Create(10m, "USD").Value;
        var euros = Money.Create(10m, "EUR").Value;

        var result = dollars.Subtract(euros);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MoneyExtensions.CurrencyMismatchMessage);
    }

    [Fact]
    public void Add_WhenResultExceedsDecimalRange_ReturnsOverflowFailure()
    {
        var maximum = Money.Create(decimal.MaxValue, "USD").Value;

        var result = maximum.Add(Money.Create(1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void Add_WhenResultFallsBelowDecimalRange_ReturnsOverflowFailure()
    {
        var minimum = Money.Create(decimal.MinValue, "USD").Value;

        var result = minimum.Add(Money.Create(-1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void Subtract_WhenResultExceedsDecimalRange_ReturnsOverflowFailure()
    {
        var maximum = Money.Create(decimal.MaxValue, "USD").Value;

        var result = maximum.Subtract(Money.Create(-1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void Subtract_WhenResultFallsBelowDecimalRange_ReturnsOverflowFailure()
    {
        var minimum = Money.Create(decimal.MinValue, "USD").Value;

        var result = minimum.Subtract(Money.Create(1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Theory]
    [InlineData(4.5, 3, 13.5)]
    [InlineData(4.5, -2, -9)]
    [InlineData(-4.5, -2, 9)]
    [InlineData(4.5, 0, 0)]
    [InlineData(5, 0.5, 2.5)]
    public void Multiply_WhenResultIsInRange_ReturnsProductAndPreservesCurrency(
        decimal amount,
        decimal multiplier,
        decimal expectedAmount)
    {
        var money = Money.Create(amount, "JPY").Value;

        var result = money.Multiply(multiplier);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Money.Create(expectedAmount, "JPY").Value);
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 2)]
    [InlineData(true, -2)]
    [InlineData(false, -2)]
    public void Multiply_WhenProductIsOutsideDecimalRange_ReturnsOverflowFailure(
        bool useMaximum,
        decimal multiplier)
    {
        var amount = useMaximum ? decimal.MaxValue : decimal.MinValue;
        var money = Money.Create(amount, "USD").Value;

        var result = money.Multiply(multiplier);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Theory]
    [InlineData(4.5, -4.5)]
    [InlineData(-4.5, 4.5)]
    [InlineData(0, 0)]
    public void Negate_WhenAmountIsAboveDecimalMinimum_ReturnsOppositeAndPreservesCurrency(
        decimal amount,
        decimal expectedAmount)
    {
        var money = Money.Create(amount, "AUD").Value;

        var result = money.Negate();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Money.Create(expectedAmount, "AUD").Value);
    }

    [Fact]
    public void Negate_WhenAmountIsDecimalMinimum_ReturnsOverflowFailure()
    {
        var minimum = Money.Create(decimal.MinValue, "USD").Value;

        var result = minimum.Negate();

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ToDisplayString_WhenCalled_UsesInvariantAmountAndCurrencyCode()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var money = Money.Create(1234.50m, "eur").Value;

            money.ToDisplayString().ShouldBe("1234.50 EUR");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void CurrencyMismatchMessage_WhenRead_DescribesSameCurrencyRequirement()
    {
        MoneyExtensions.CurrencyMismatchMessage.ShouldBe(
            "Money values must have the same currency.");
    }
}
