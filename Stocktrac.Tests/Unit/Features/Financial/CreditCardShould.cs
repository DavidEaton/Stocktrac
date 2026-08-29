using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class CreditCardShould
{
    [Fact]
    public void Normalize_Valid_Names_When_Created()
    {
        var result = CreateCreditCard("  Visa  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.Name.Value.ShouldBe("Visa");
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
    public void Accept_Names_At_Both_Length_Boundaries()
    {
        CreateCreditCard(new string('a', CreditCard.MinimumLength)).IsSuccess.ShouldBe(true);
        CreateCreditCard(new string('a', CreditCard.MaximumLength)).IsSuccess.ShouldBe(true);
    }

    [Fact]
    public void Normalize_A_Valid_Name_When_Changed()
    {
        var creditCard = CreateCreditCard("Visa").Value;

        var result = creditCard.SetName("  Mastercard  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe("Mastercard");
        creditCard.Name.ShouldBeOfType<CreditCardName>();
        creditCard.Name.Value.ShouldBe("Mastercard");
    }

    [Fact]
    public void Preserve_The_Name_When_A_Change_Is_Invalid()
    {
        var creditCard = CreateCreditCard("Visa").Value;

        var result = creditCard.SetName(null);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCard.RequiredMessage);
        creditCard.Name.Value.ShouldBe("Visa");
    }

    [Fact]
    public void Create_With_All_Requested_Values()
    {
        var name = CreditCardName.Create("Visa").Value;
        var fee = Fee.Create(2.5m, "USD");
        var depositedAt = new DateTime(2026, 8, 29, 12, 30, 0, DateTimeKind.Utc);

        var card = CreditCard.Create(name, CreditCardFeeType.Percentage, fee, depositedAt).Value;

        card.Name.ShouldBe(name);
        card.FeeType.ShouldBe(CreditCardFeeType.Percentage);
        card.Fee.ShouldBe(fee);
        card.AddedToDeposit.ShouldBe(depositedAt);
        card.IsAddedToDeposit().ShouldBe(true);
    }

    [Theory]
    [InlineData(CreditCardFeeType.None)]
    [InlineData(CreditCardFeeType.Percentage)]
    [InlineData(CreditCardFeeType.Flat)]
    public void Set_Defined_Fee_Types(CreditCardFeeType feeType)
    {
        var card = CreateCreditCard("Visa").Value;

        var result = card.SetFeeType(feeType);

        result.IsSuccess.ShouldBe(true);
        card.FeeType.ShouldBe(feeType);
    }

    [Fact]
    public void Preserve_Fee_Type_When_An_Undefined_Value_Is_Set()
    {
        var card = CreateCreditCard("Visa").Value;

        var result = card.SetFeeType((CreditCardFeeType)999);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCard.RequiredMessage);
        card.FeeType.ShouldBe(CreditCardFeeType.Flat);
    }

    [Fact]
    public void Set_Fee_And_Deposit_Date()
    {
        var card = CreateCreditCard("Visa").Value;
        var fee = Fee.Create(3m, "CAD");
        var depositedAt = new DateTime(2026, 8, 29);

        card.SetFee(fee).Value.ShouldBe(fee);
        card.SetAddedToDeposit(depositedAt).Value.ShouldBe(depositedAt);
        card.Fee.ShouldBe(fee);
        card.IsAddedToDeposit().ShouldBe(true);
    }

    [Fact]
    public void Replace_Name_With_A_Validated_Value_Object()
    {
        var card = CreateCreditCard("Visa").Value;
        var name = CreditCardName.Create("Mastercard").Value;

        card.SetName(name).Value.ShouldBe(name);
        card.Name.ShouldBe(name);
    }

    private static Result<CreditCard> CreateCreditCard(string? name)
    {
        var cardNameResult = CreditCardName.Create(name);

        return cardNameResult.IsFailure
            ? Result.Failure<CreditCard>(cardNameResult.Error)
            : CreditCard.Create(
                cardNameResult.Value,
                CreditCardFeeType.Flat,
                Fee.Default,
                DateTime.MinValue);
    }
}
