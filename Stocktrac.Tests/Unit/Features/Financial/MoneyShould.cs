using Shouldly;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class MoneyShould
{
    [Fact]
    public void Treat_All_Decimal_Values_As_Valid_Signed_Amounts()
    {
        var values = new[] { decimal.MinValue, -1m, 0m, 1m, decimal.MaxValue };

        foreach (var value in values)
            Amount.FromDecimal(value).Value.ShouldBe(value);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData(" EUR ", "EUR")]
    public void Normalize_Currency_Codes(string input, string expected)
    {
        var result = CurrencyCode.Create(input);

        result.IsSuccess.ShouldBe(true);
        result.Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("US1")]
    public void Reject_Invalid_Currency_Codes(string? input)
    {
        CurrencyCode.Create(input).IsFailure.ShouldBe(true);
    }

    [Theory]
    [InlineData("XCG")]
    [InlineData("ZWG")]
    [InlineData("XAU")]
    public void Accept_Codes_From_The_Embedded_Iso_4217_List(string input)
    {
        CurrencyCode.Create(input).IsSuccess.ShouldBe(true);
    }

    [Fact]
    public void Reject_A_Well_Formed_Code_That_Is_Not_In_The_Iso_4217_List()
    {
        var result = CurrencyCode.Create("ZZZ");

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CurrencyCode.UnsupportedMessage);
    }

    [Fact]
    public void Create_A_Strongly_Typed_Value()
    {
        var amount = Amount.FromDecimal(12.34m);
        var currencyCode = CurrencyCode.Create("USD").Value;

        var result = Money.Create(amount, currencyCode);

        result.ShouldBeOfType<Money>();
        result.Amount.ShouldBe(Amount.FromDecimal(12.34m));
        result.CurrencyCode.Value.ShouldBe("USD");
        result.ToDisplayString().ShouldBe("12.34 USD");
    }

    [Fact]
    public void Use_Value_Equality()
    {
        var first = Money.Create(10m, "USD").Value;
        var second = Money.Create(10m, "usd").Value;

        first.ShouldBe(second);
    }

    [Fact]
    public void Add_Values_With_The_Same_Currency()
    {
        var left = Money.Create(10.25m, "USD").Value;
        var right = Money.Create(2.75m, "USD").Value;

        var result = left.Add(right);

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe(Money.Create(13m, "USD").Value);
    }

    [Fact]
    public void Subtract_Values_With_The_Same_Currency()
    {
        var left = Money.Create(10.25m, "USD").Value;
        var right = Money.Create(2.75m, "USD").Value;

        left.Subtract(right).Value.ShouldBe(Money.Create(7.50m, "USD").Value);
    }

    [Fact]
    public void Reject_Arithmetic_Between_Different_Currencies()
    {
        var dollars = Money.Create(10m, "USD").Value;
        var euros = Money.Create(10m, "EUR").Value;

        var result = dollars.Subtract(euros);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(MoneyExtensions.CurrencyMismatchMessage);
    }

    [Fact]
    public void Reject_Addition_Between_Different_Currencies()
    {
        var dollars = Money.Create(10m, "USD").Value;
        var euros = Money.Create(10m, "EUR").Value;

        var result = dollars.Add(euros);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(MoneyExtensions.CurrencyMismatchMessage);
    }

    [Fact]
    public void Multiply_And_Negate_While_Preserving_Currency()
    {
        var money = Money.Create(4.50m, "GBP").Value;

        money.Multiply(3m).Value.ShouldBe(Money.Create(13.50m, "GBP").Value);
        money.Negate().Value.ShouldBe(Money.Create(-4.50m, "GBP").Value);
    }

    [Fact]
    public void Return_A_Failure_When_Arithmetic_Overflows()
    {
        var maximum = Amount.FromDecimal(decimal.MaxValue);

        var result = maximum.Add(Amount.FromDecimal(1m));

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void Return_Failures_For_Every_Overflowing_Amount_Operation()
    {
        Amount.FromDecimal(decimal.MinValue).Subtract(Amount.FromDecimal(1m)).Error
            .ShouldBe(Amount.OverflowMessage);
        Amount.FromDecimal(decimal.MaxValue).Multiply(2m).Error
            .ShouldBe(Amount.OverflowMessage);
        Amount.FromDecimal(decimal.MinValue).Negate().Error
            .ShouldBe(Amount.OverflowMessage);
    }

    [Fact]
    public void Propagate_Overflow_Failures_Through_Money_Operations()
    {
        var maximum = Money.Create(decimal.MaxValue, "USD").Value;

        maximum.Add(Money.Create(1m, "USD").Value).Error
            .ShouldBe(Amount.OverflowMessage);
        maximum.Multiply(2m).Error.ShouldBe(Amount.OverflowMessage);
    }

    [Theory]
    [InlineData(null, CurrencyCode.InvalidMessage)]
    [InlineData("US1", CurrencyCode.InvalidMessage)]
    [InlineData("ZZZ", CurrencyCode.UnsupportedMessage)]
    public void Propagate_Currency_Errors_When_Creating_Money(string? code, string expectedError)
    {
        var result = Money.Create(1m, code);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(expectedError);
    }

    [Fact]
    public void Format_Amounts_Using_The_Invariant_Culture()
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture =
                System.Globalization.CultureInfo.GetCultureInfo("fr-FR");

            Amount.FromDecimal(12.34m).ToString().ShouldBe("12.34");
            Money.Create(12.34m, "USD").Value.ToDisplayString().ShouldBe("12.34 USD");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Create_Usd_Default_Value()
    {
        default(CurrencyCode).ShouldBe(CurrencyCode.Usd);
        default(CurrencyCode).Value.ShouldBe("USD");
    }

    [Fact]
    public void Create_Usd_With_Parameterless_Constructor()
    {
        new CurrencyCode().ShouldBe(CurrencyCode.Usd);
    }

    [Fact]
    public void Create_Explicit_Usd_Default_Value()
    {
        CurrencyCode.Create("usd").Value
            .ShouldBe(default);
    }
}
