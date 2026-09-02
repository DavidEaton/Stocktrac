using System.Globalization;
using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class AmountShould
{
    [Theory]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("-12.34")]
    [InlineData("0")]
    [InlineData("12.34")]
    [InlineData("79228162514264337593543950335")]
    public void ContainExactValue_On_FromDecimal_WhenGivenAnyDecimal(string input)
    {
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        var amount = Amount.FromDecimal(value);

        amount.Value.ShouldBe(value);
    }

    [Fact]
    public void ContainZeroValue_WhenDefault()
    {
        default(Amount).Value.ShouldBe(0m);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = Amount.FromDecimal(12.340m);
        var second = Amount.FromDecimal(12.34m);

        (first == second).ShouldBeTrue();
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void NotBeEqual_WhenValuesDiffer()
    {
        var first = Amount.FromDecimal(12.34m);
        var second = Amount.FromDecimal(12.35m);

        (first != second).ShouldBeTrue();
        first.Equals(second).ShouldBeFalse();
    }

    [Fact]
    public void ReturnInvariantRepresentation_On_ToString_WhenCurrentCultureUsesComma()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            Amount.FromDecimal(-1234.50m).ToString().ShouldBe("-1234.50");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("10.25", "2.75", "13.00")]
    [InlineData("-10", "3", "-7")]
    [InlineData("-10", "-3", "-13")]
    [InlineData("0", "0", "0")]
    [InlineData("79228162514264337593543950335", "0", "79228162514264337593543950335")]
    public void ReturnSuccessfulSum_On_Add_WhenResultIsRepresentable(
        string leftInput,
        string rightInput,
        string expectedInput)
    {
        var left = CreateAmount(leftInput);
        var right = CreateAmount(rightInput);

        var result = left.Add(right);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateAmount(expectedInput));
    }

    [Theory]
    [InlineData("79228162514264337593543950335", "1")]
    [InlineData("-79228162514264337593543950335", "-1")]
    public void ReturnOverflowFailure_On_Add_WhenResultExceedsDecimalRange(
        string leftInput,
        string rightInput)
    {
        var result = CreateAmount(leftInput).Add(CreateAmount(rightInput));

        AssertOverflowFailure(result.IsFailure, result.Error);
    }

    [Theory]
    [InlineData("10.25", "2.75", "7.50")]
    [InlineData("-10", "3", "-13")]
    [InlineData("-10", "-3", "-7")]
    [InlineData("0", "0", "0")]
    [InlineData("-79228162514264337593543950335", "0", "-79228162514264337593543950335")]
    public void ReturnSuccessfulDifference_On_Subtract_WhenResultIsRepresentable(
        string leftInput,
        string rightInput,
        string expectedInput)
    {
        var result = CreateAmount(leftInput).Subtract(CreateAmount(rightInput));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateAmount(expectedInput));
    }

    [Theory]
    [InlineData("79228162514264337593543950335", "-1")]
    [InlineData("-79228162514264337593543950335", "1")]
    public void ReturnOverflowFailure_On_Subtract_WhenResultExceedsDecimalRange(
        string leftInput,
        string rightInput)
    {
        var result = CreateAmount(leftInput).Subtract(CreateAmount(rightInput));

        AssertOverflowFailure(result.IsFailure, result.Error);
    }

    [Theory]
    [InlineData("4.50", "3", "13.50")]
    [InlineData("4.50", "-2", "-9.00")]
    [InlineData("-4.50", "-2", "9.00")]
    [InlineData("79228162514264337593543950335", "0", "0")]
    [InlineData("79228162514264337593543950335", "1", "79228162514264337593543950335")]
    [InlineData("0.0000000000000000000000000001", "0.1", "0")]
    public void ReturnSuccessfulProduct_On_Multiply_WhenResultIsRepresentable(
        string amountInput,
        string multiplierInput,
        string expectedInput)
    {
        var multiplier = decimal.Parse(multiplierInput, CultureInfo.InvariantCulture);

        var result = CreateAmount(amountInput).Multiply(multiplier);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateAmount(expectedInput));
    }

    [Theory]
    [InlineData("79228162514264337593543950335", "2")]
    [InlineData("-79228162514264337593543950335", "2")]
    public void ReturnOverflowFailure_On_Multiply_WhenResultExceedsDecimalRange(
        string amountInput,
        string multiplierInput)
    {
        var multiplier = decimal.Parse(multiplierInput, CultureInfo.InvariantCulture);

        var result = CreateAmount(amountInput).Multiply(multiplier);

        AssertOverflowFailure(result.IsFailure, result.Error);
    }

    [Theory]
    [InlineData("12.34", "-12.34")]
    [InlineData("-12.34", "12.34")]
    [InlineData("0", "0")]
    [InlineData("79228162514264337593543950335", "-79228162514264337593543950335")]
    public void ReturnSuccessfulOpposite_On_Negate_WhenResultIsRepresentable(
        string input,
        string expectedInput)
    {
        var result = CreateAmount(input).Negate();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(CreateAmount(expectedInput));
    }

    [Fact]
    public void ReturnNegatedAmount_On_Negate_WhenAmountIsDecimalMinimum()
    {
        var result = Amount.FromDecimal(decimal.MinValue).Negate();

        result.Value.ShouldBe(Amount.FromDecimal(decimal.MaxValue));
    }

    private static Amount CreateAmount(string input) =>
        Amount.FromDecimal(decimal.Parse(input, CultureInfo.InvariantCulture));

    private static void AssertOverflowFailure(bool isFailure, string error)
    {
        isFailure.ShouldBeTrue();
        error.ShouldBe(Amount.OverflowMessage);
    }
}
