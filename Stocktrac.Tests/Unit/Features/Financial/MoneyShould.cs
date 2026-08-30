using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class MoneyShould
{
    [Fact]
    public void SetAmountAndCurrency_On_Create_WhenGivenAmountAndCurrencyCode()
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
    public void ReturnMoneyWithNormalizedCurrency_On_Create_WhenGivenValidCurrencyText(
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
    public void ReturnCurrencyFailure_On_Create_WhenGivenInvalidCurrencyText(
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
    public void ReturnMoney_On_Create_WhenGivenAnyDecimalAmount(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var result = Money.Create(amount, "USD");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.Value.ShouldBe(amount);
    }

    [Fact]
    public void ReturnZeroUsd_WhenDefault()
    {
        var money = default(Money);

        money.Amount.ShouldBe(Amount.FromDecimal(0m));
        money.CurrencyCode.ShouldBe(CurrencyCode.Usd);
    }

    [Fact]
    public void BeEqual_WhenAmountAndNormalizedCurrencyAreEqual()
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
    public void NotBeEqual_WhenAmountOrCurrencyDiffers(
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
    public void ReturnSumAndPreserveCurrency_On_Add_WhenCurrenciesMatch(
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
    public void ReturnDifferenceAndPreserveCurrency_On_Subtract_WhenCurrenciesMatch(
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
    public void ReturnCurrencyMismatchFailure_On_Add_WhenCurrenciesDiffer()
    {
        var dollars = Money.Create(10m, "USD").Value;
        var euros = Money.Create(10m, "EUR").Value;

        var result = dollars.Add(euros);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MoneyExtensions.CurrencyMismatchMessage);
    }

    [Fact]
    public void ReturnCurrencyMismatchFailure_On_Subtract_WhenCurrenciesDiffer()
    {
        var dollars = Money.Create(10m, "USD").Value;
        var euros = Money.Create(10m, "EUR").Value;

        var result = dollars.Subtract(euros);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MoneyExtensions.CurrencyMismatchMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Add_WhenResultExceedsDecimalRange()
    {
        var maximum = Money.Create(decimal.MaxValue, "USD").Value;

        var result = maximum.Add(Money.Create(1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Add_WhenResultFallsBelowDecimalRange()
    {
        var minimum = Money.Create(decimal.MinValue, "USD").Value;

        var result = minimum.Add(Money.Create(-1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Subtract_WhenResultExceedsDecimalRange()
    {
        var maximum = Money.Create(decimal.MaxValue, "USD").Value;

        var result = maximum.Subtract(Money.Create(-1m, "USD").Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Subtract_WhenResultFallsBelowDecimalRange()
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
    public void ReturnProductAndPreserveCurrency_On_Multiply_WhenResultIsInRange(
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
    public void ReturnOverflowFailure_On_Multiply_WhenProductIsOutsideDecimalRange(
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
    public void ReturnOppositeAndPreserveCurrency_On_Negate_WhenAmountIsAboveDecimalMinimum(
        decimal amount,
        decimal expectedAmount)
    {
        var money = Money.Create(amount, "AUD").Value;

        var result = money.Negate();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Money.Create(expectedAmount, "AUD").Value);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Negate_WhenAmountIsDecimalMinimum()
    {
        var minimum = Money.Create(decimal.MinValue, "USD").Value;

        var result = minimum.Negate();

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void UseInvariantAmountAndCurrencyCode_On_ToDisplayString()
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
    public void DescribeSameCurrencyRequirement_In_CurrencyMismatchMessage()
    {
        MoneyExtensions.CurrencyMismatchMessage.ShouldBe(
            "Money values must have the same currency.");
    }
}
