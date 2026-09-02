using Shouldly;
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
        result.Value.AddedToDeposit.ShouldBe(depositedAt);
    }

    [Fact]
    public void RejectUndefinedFeeType_On_Create()
    {
        var result = CreditCard.Create(
            CreateName("Visa"),
            (CreditCardFeeType)999,
            Fee.Default,
            null);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCard.InvalidFeeTypeMessage);
    }

    [Fact]
    public void TrimAndSetName_On_SetName_WhenStringIsValid()
    {
        var card = CreateCreditCard();

        var result = card.SetName("  Mastercard  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(CreateName("Mastercard"));
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PreserveName_On_SetName_WhenStringIsMissing(string? name)
    {
        var card = CreateCreditCard();

        var result = card.SetName(name);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.RequiredMessage);
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Fact]
    public void PreserveName_On_SetName_WhenStringIsTooLong()
    {
        var card = CreateCreditCard();

        var result = card.SetName(new string('a', CreditCardName.MaximumLength + 1));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCardName.InvalidLengthMessage);
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Theory]
    [InlineData(CreditCardName.MinimumLength)]
    [InlineData(CreditCardName.MaximumLength)]
    public void SetName_On_SetName_WhenStringIsAtLengthBoundary(int length)
    {
        var card = CreateCreditCard();
        var name = new string('a', length);

        var result = card.SetName(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe(name);
        card.Name.Value.ShouldBe("Visa");
    }

    [Fact]
    public void ReplaceName_On_SetName_WhenGivenCreditCardName()
    {
        var card = CreateCreditCard();
        var name = CreateName("Mastercard");

        var result = card.SetName(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
        card.Name.ShouldBe(CreateName("Visa"));
    }

    [Theory]
    [InlineData(CreditCardFeeType.None)]
    [InlineData(CreditCardFeeType.Percentage)]
    [InlineData(CreditCardFeeType.Flat)]
    public void SetFeeType_On_SetFeeType_WhenFeeTypeIsDefined(CreditCardFeeType feeType)
    {
        var card = CreateCreditCard();

        var result = card.SetFeeType(feeType);

        result.IsSuccess.ShouldBeTrue();
        result.Value.FeeType.ShouldBe(feeType);
        card.FeeType.ShouldBe(CreditCardFeeType.Flat);
    }

    [Fact]
    public void PreserveFeeType_On_SetFeeType_WhenFeeTypeIsUndefined()
    {
        var card = CreateCreditCard();

        var result = card.SetFeeType((CreditCardFeeType)999);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CreditCard.InvalidFeeTypeMessage);
        card.FeeType.ShouldBe(CreditCardFeeType.Flat);
    }

    [Fact]
    public void ReplaceFee_On_SetFee()
    {
        var card = CreateCreditCard();
        var fee = Fee.Create(3m, "CAD").Value;

        var result = card.SetFee(fee);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Fee.ShouldBe(fee);
        card.Fee.ShouldBe(Fee.Default);
    }

    [Fact]
    public void ReplaceDepositDate_On_SetAddedToDeposit()
    {
        var card = CreateCreditCard();
        var depositedAt = new DateTime(2026, 8, 30, 10, 15, 0, DateTimeKind.Utc);

        var result = card.SetAddedToDeposit(depositedAt);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddedToDeposit.ShouldBe(depositedAt);
        card.AddedToDeposit.ShouldBe(DateTime.MinValue);
    }

    [Fact]
    public void ReportWhetherItHasBeenAddedToDeposit_On_IsAddedToDeposit()
    {
        CreditCard.Create(
            CreateName("Visa"),
            CreditCardFeeType.Flat,
            Fee.Default,
            null).Value.IsAddedToDeposit.ShouldBeFalse();
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
