using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Financial;
using Stocktrac.Domain.Features.Financial.Extensions;

namespace Stocktrac.Tests.Unit.Features.Financial;

public class CreditCardShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectName_On_Create_WhenNameIsMissing(string? name)
    {
        var result = CreateCreditCard(name);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCardName.RequiredMessage);
    }

    [Theory]
    [InlineData("1234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011")]
    [InlineData("12345678901234567890123456789012345678901234567890112345678901234567890123456789012345678901234567890112345678901234567890123456789012345678901234567890112345678901234567890123456789012345678901234567890112345678901234567890123456789012345678901234567890111234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011234567890123456789012345678901234567890123456789011")]
    public void RejectName_On_Create_WhenNameIsOutsideAllowedLength(string name)
    {
        var result = CreateCreditCard(name);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
    }

    [Fact]
    public void AcceptName_On_Create_WhenNameIsAtLengthBoundary()
    {
        CreateCreditCard(new string('a', CreditCardName.MinimumLength)).IsSuccess.ShouldBe(true);
        CreateCreditCard(new string('a', CreditCardName.MaximumLength)).IsSuccess.ShouldBe(true);
    }

    [Fact]
    public void NormalizeName_On_SetName_WhenNameIsValid()
    {
        var creditCard = CreateCreditCard("Visa").Value;

        var result = creditCard.SetName("  Mastercard  ");

        result.IsSuccess.ShouldBe(true);
        result.Value.ShouldBe("Mastercard");
        creditCard.Name.ShouldBeOfType<CreditCardName>();
        creditCard.Name.Value.ShouldBe("Mastercard");
    }

    [Fact]
    public void PreserveName_On_SetName_WhenNameIsInvalid()
    {
        var creditCard = CreateCreditCard("Visa").Value;

        var result = creditCard.SetName(null);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
        creditCard.Name.Value.ShouldBe("Visa");
    }

    [Fact]
    public void SetAllRequestedValues_On_Create_WhenValuesAreValid()
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
    public void SetFeeType_On_SetFeeType_WhenFeeTypeIsDefined(CreditCardFeeType feeType)
    {
        var card = CreateCreditCard("Visa").Value;

        var result = card.SetFeeType(feeType);

        result.IsSuccess.ShouldBe(true);
        card.FeeType.ShouldBe(feeType);
    }

    [Fact]
    public void PreserveFeeType_On_SetFeeType_WhenFeeTypeIsUndefined()
    {
        var card = CreateCreditCard("Visa").Value;

        var result = card.SetFeeType((CreditCardFeeType)999);

        result.IsFailure.ShouldBe(true);
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
        card.FeeType.ShouldBe(CreditCardFeeType.Flat);
    }

    [Fact]
    public void SetFeeAndDepositDate_On_SetFeeAndDepositDate_WhenValuesAreValid()
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
    public void ReplaceName_On_SetName_WhenGivenValidatedName()
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
