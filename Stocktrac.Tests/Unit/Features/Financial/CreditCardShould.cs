using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class CreditCardShould
{
    [Fact]
    public void Normalize_Valid_Names_When_Created()
    {
        var result = CreateCreditCard("  Visa  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.Name.ShouldBe("Visa");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_Missing_Names_When_Created(string? name)
    {
        var result = CreateCreditCard(name);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCard.RequiredMessage);
    }

    [Theory]
    [InlineData("V")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    public void Reject_Names_Outside_The_Allowed_Length(string name)
    {
        var result = CreateCreditCard(name);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCard.InvalidLengthMessage);
    }

    [Fact]
    public void Normalize_A_Valid_Name_When_Changed()
    {
        var creditCard = CreateCreditCard("Visa").Value;

        var result = creditCard.SetName("  Mastercard  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe("Mastercard");
        creditCard.Name.ShouldBe("Mastercard");
    }

    [Fact]
    public void Preserve_The_Name_When_A_Change_Is_Invalid()
    {
        var creditCard = CreateCreditCard("Visa").Value;

        var result = creditCard.SetName(null);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCard.RequiredMessage);
        creditCard.Name.ShouldBe("Visa");
    }

    private static Result<CreditCard> CreateCreditCard(string? name) =>
        CreditCard.Create(name, CreditCardFeeType.Flat, 0, false);
}
