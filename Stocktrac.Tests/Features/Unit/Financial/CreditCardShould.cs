using Shouldly;
using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Financial;

namespace Stocktrac.Tests.Features.Unit.Financial;

public class CreditCardShould
{
    [Fact]
    public void SetAllRequestedValues_On_Create()
    {
        var name = CreateName("Visa");
        var fee = Fee.Create(2.5m, "USD").Value;
        var depositedAt = new DateTime(2026, 8, 29, 12, 30, 0, DateTimeKind.Utc);

        var result = CreditCard.Create(
            name,
            CreditCardFeeType.Percentage,
            fee,
            depositedAt);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
        result.Value.FeeType.ShouldBe(CreditCardFeeType.Percentage);
        result.Value.Fee.ShouldBe(fee);
        result.Value.AddedToDeposit.Value.ShouldBe(depositedAt);
    }

    [Fact]
    public void RejectUndefinedFeeType_On_Create()
    {
        var result = CreditCard.Create(
            CreateName("Visa"),
            (CreditCardFeeType)999,
            Fee.Default,
            Maybe<DateTime>.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCard.InvalidFeeTypeMessage);
    }

    [Fact]
    public void TrimAndReplaceName_On_WithName_WhenStringIsValid()
    {
        var card = CreateCreditCard();

        var result = card.WithName("  Mastercard  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(CreateName("Mastercard"));
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PreserveName_On_WithName_WhenStringIsMissing(string name)
    {
        var card = CreateCreditCard();

        var result = card.WithName(name);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.RequiredMessage);
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Fact]
    public void PreserveName_On_WithName_WhenStringIsTooLong()
    {
        var card = CreateCreditCard();

        var result = card.WithName(new string('a', CreditCardName.MaximumLength + 1));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Theory]
    [InlineData(CreditCardName.MinimumLength)]
    [InlineData(CreditCardName.MaximumLength)]
    public void ReplaceName_On_WithName_WhenStringIsAtLengthBoundary(int length)
    {
        var card = CreateCreditCard();
        var name = new string('a', length);

        var result = card.WithName(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe(name);
        card.Name.Value.ShouldBe("Visa");
    }

    [Fact]
    public void ReplaceName_On_WithName_WhenGivenCreditCardName()
    {
        var card = CreateCreditCard();
        var name = CreateName("Mastercard");

        var result = card.WithName(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Theory]
    [InlineData((int)CreditCardFeeType.None)]
    [InlineData((int)CreditCardFeeType.Percentage)]
    [InlineData((int)CreditCardFeeType.Flat)]
    public void ReplaceFeeType_On_WithFeeType_WhenFeeTypeIsDefined(int feeTypeValue)
    {
        var card = CreateCreditCard();
        var feeType = (CreditCardFeeType)feeTypeValue;
        
        var result = card.WithFeeType(feeType);
        
        result.IsSuccess.ShouldBeTrue();
        result.Value.FeeType.ShouldBe(feeType);
        card.FeeType.ShouldBe(CreditCardFeeType.Flat);
    }

    [Fact]
    public void PreserveFeeType_On_WithFeeType_WhenFeeTypeIsUndefined()
    {
        var card = CreateCreditCard();

        var result = card.WithFeeType((CreditCardFeeType)999);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCard.InvalidFeeTypeMessage);
        card.FeeType.ShouldBe(CreditCardFeeType.Flat);
    }

    [Fact]
    public void ReplaceFee_On_WithFee()
    {
        var card = CreateCreditCard();
        var fee = Fee.Create(3m, "CAD").Value;

        var result = card.WithFee(fee);

        result.Value.Fee.ShouldBe(fee);
        card.Fee.ShouldBe(Fee.Default);
    }

    [Fact]
    public void ReplaceDepositDate_On_WithAddedToDeposit()
    {
        var card = CreateCreditCard();
        var depositedAt = new DateTime(2026, 8, 30, 10, 15, 0, DateTimeKind.Utc);

        var result = card.WithAddedToDeposit(depositedAt);

        result.AddedToDeposit.Value.ShouldBe(depositedAt);
        card.AddedToDeposit.Value.ShouldBe(DateTime.MinValue);
    }

    [Fact]
    public void RemoveDepositDate_On_WithoutAddedToDeposit()
    {
        var card = CreateCreditCard();

        var result = card.WithoutAddedToDeposit();

        result.AddedToDeposit.HasNoValue.ShouldBeTrue();
        result.IsAddedToDeposit.ShouldBeFalse();
        card.AddedToDeposit.HasValue.ShouldBeTrue();
    }

    [Fact]
    public void ReportWhetherItHasBeenAddedToDeposit_On_IsAddedToDeposit()
    {
        CreditCard.Create(
            CreateName("Visa"),
            CreditCardFeeType.Flat,
            Fee.Default,
            Maybe<DateTime>.None).Value.AddedToDeposit.HasNoValue.ShouldBeTrue();
        CreateCreditCard().IsAddedToDeposit.ShouldBeTrue();
    }

    private static CreditCard CreateCreditCard() =>
        CreditCard.Create(
            CreateName("Visa"),
            CreditCardFeeType.Flat,
            Fee.Default,
            DateTime.MinValue).Value;

    private static CreditCardName CreateName(string name) =>
        CreditCardName.Create(name).Value;
}
