using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class MoneyShould
{
    [Fact]
    public void SetAmountAndCurrency_On_Create_WhenGivenAmountAndCurrencyCode()
    {
        var amount = Amount.FromDecimal(12.34m);
        var currencyCode = CurrencyCode.Create(NonEmptyString.Create("CAD").Value).Value;

        var money = Money.Create(amount, currencyCode);

        money.Value.Amount.ShouldBe(amount);
        money.Value.CurrencyCode.ShouldBe(currencyCode);
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

        var result = Money.Create(Amount.FromDecimal(amount), CurrencyCode.Usd);

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
    public void BeEqual_WhenAmountAndCurrencyAreEqual()
    {
        var first = CreateMoney(10m, "USD");
        var second = CreateMoney(10m, "USD");

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
        var first = CreateMoney(firstAmount, firstCurrency);
        var second = CreateMoney(secondAmount, secondCurrency);

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
        var left = CreateMoney(leftAmount, "CAD");
        var right = CreateMoney(rightAmount, "CAD");

        var result = left.Add(right);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateMoney(expectedAmount, "CAD"));
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
        var left = CreateMoney(leftAmount, "GBP");
        var right = CreateMoney(rightAmount, "GBP");

        var result = left.Subtract(right);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateMoney(expectedAmount, "GBP"));
    }

    [Fact]
    public void ReturnCurrencyMismatchFailure_On_Add_WhenCurrenciesDiffer()
    {
        var dollars = CreateMoney(10m, "USD");
        var euros = CreateMoney(10m, "EUR");

        var result = dollars.Add(euros);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Money.CurrencyMismatchMessage);
    }

    [Fact]
    public void ReturnCurrencyMismatchFailure_On_Subtract_WhenCurrenciesDiffer()
    {
        var dollars = CreateMoney(10m, "USD");
        var euros = CreateMoney(10m, "EUR");

        var result = dollars.Subtract(euros);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Money.CurrencyMismatchMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Add_WhenResultExceedsDecimalRange()
    {
        var maximum = CreateMoney(decimal.MaxValue, "USD");

        var result = maximum.Add(CreateMoney(1m, "USD"));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Add_WhenResultFallsBelowDecimalRange()
    {
        var minimum = CreateMoney(decimal.MinValue, "USD");

        var result = minimum.Add(CreateMoney(-1m, "USD"));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Subtract_WhenResultExceedsDecimalRange()
    {
        var maximum = CreateMoney(decimal.MaxValue, "USD");

        var result = maximum.Subtract(CreateMoney(-1m, "USD"));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void ReturnOverflowFailure_On_Subtract_WhenResultFallsBelowDecimalRange()
    {
        var minimum = CreateMoney(decimal.MinValue, "USD");

        var result = minimum.Subtract(CreateMoney(1m, "USD"));

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
        var money = CreateMoney(amount, "JPY");

        var result = money.Multiply(multiplier);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateMoney(expectedAmount, "JPY"));
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
        var money = CreateMoney(amount, "USD");

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
        var money = CreateMoney(amount, "AUD");

        var result = money.Negate();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateMoney(expectedAmount, "AUD"));
    }

    [Fact]
    public void ReturnDecimalMaxValue_On_Negate_WhenAmountIsDecimalMinimum()
    {
        var minimum = CreateMoney(decimal.MinValue, "USD");

        var result = minimum.Negate();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateMoney(decimal.MaxValue, "USD"));
    }

    [Fact]
    public void DescribeSameCurrencyRequirement_In_CurrencyMismatchMessage()
    {
        Money.CurrencyMismatchMessage.ShouldBe(
            "Money values must have the same currency.");
    }

    private static Money CreateMoney(decimal amount, string currencyCode) =>
        Money.Create(Amount.FromDecimal(amount), CreateCurrency(currencyCode)).Value;

    private static CurrencyCode CreateCurrency(string currencyCode) =>
        CurrencyCode.Create(NonEmptyString.Create(currencyCode).Value).Value;
}
