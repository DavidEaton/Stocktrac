using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class PhoneShould
{
    [Fact]
    public void NormalizeAndPreserveValues_On_Create_WhenValuesAreValid()
    {
        var result = Phone.Create(" 555-123-4567 ", PhoneType.Mobile, true);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe("5551234567");
        result.Value.PhoneType.ShouldBe(PhoneType.Mobile);
        result.Value.IsPrimary.ShouldBeTrue();
    }

    [Theory]
    [InlineData(" +44 20 7946 0958 ", "+442079460958")]
    [InlineData("+61 (2) 9374-4000", "+61293744000")]
    [InlineData("+1.555.123.4567", "+15551234567")]
    public void StoreCanonicalInternationalNumber_On_Create_WhenNumberHasInternationalFormat(
        string number,
        string canonicalNumber)
    {
        var result = Phone.Create(number, PhoneType.Mobile, false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(canonicalNumber);
        result.Value.ToString().ShouldBe(canonicalNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ReturnEmptyAndInvalidErrors_OnCreate_WhenNumberIsEmpty(
        string? number)
    {
        var expected = Phone.InvalidMessage;

#pragma warning disable CS8604 // Possible null reference argument.
        var result = Phone.Create(number, PhoneType.Home, false);
#pragma warning restore CS8604 // Possible null reference argument.

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(expected);
    }

    [Fact]
    public void ReturnInvalidError_OnCreate_WhenNumberHasInvalidFormat()
    {
        var result = Phone.Create(
            "not a phone",
            PhoneType.Home,
            false);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Phone.InvalidMessage);
    }

    [Theory]
    [InlineData("12+345678")]
    [InlineData("+0123456789")]
    [InlineData("+1234567890123456")]
    public void ReturnInvalidError_On_Create_WhenInternationalNumberIsNotCanonicalizable(string number) =>
        Phone.Create(number, PhoneType.Home, false).Error.ShouldBe(Phone.InvalidMessage);

    [Fact]
    public void ReturnPhoneTypeError_On_Create_WhenPhoneTypeIsUndefined() =>
        Phone.Create("5551234567", (PhoneType)99, false).Error.ShouldBe(Phone.PhoneTypeInvalidMessage);

    [Theory]
    [InlineData("5551234", "555-1234")]
    [InlineData("555.123.4567", "(555) 123-4567")]
    [InlineData("+1 (555) 123-4567", "+15551234567")]
    public void ReturnNumericFormattedValue_On_ToString_WhenNumberIsValid(string number, string formatted) =>
        Phone.Create(number, PhoneType.Home, false).Value.ToString().ShouldBe(formatted);

    [Fact]
    public void ReturnUpdatedCopy_On_WithNumber_WhenNumberIsValid()
    {
        var original = ValidPhone();
        var updated = original.WithNumber(" 555-987-6543 ").Value;

        updated.Number.ShouldBe("5559876543");
        updated.PhoneType.ShouldBe(original.PhoneType);
        updated.IsPrimary.ShouldBe(original.IsPrimary);
        original.Number.ShouldBe("5551234567");
    }

    [Fact]
    public void StoreCanonicalInternationalNumber_On_WithNumber_WhenNumberHasInternationalFormat()
    {
        var updated = ValidPhone().WithNumber("+33 (1) 42 68 53 00").Value;

        updated.Number.ShouldBe("+33142685300");
    }

    [Fact]
    public void ReturnErrorAndLeaveOriginalUnchanged_On_WithNumber_WhenNumberIsInvalid()
    {
        var original = ValidPhone();
        original.WithNumber("invalid").Error.ShouldBe(Phone.InvalidMessage);
        original.Number.ShouldBe("5551234567");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithPhoneType_WhenPhoneTypeIsDefined() =>
        ValidPhone().WithPhoneType(PhoneType.Work).Value.PhoneType.ShouldBe(PhoneType.Work);

    [Fact]
    public void ReturnError_On_WithPhoneType_WhenPhoneTypeIsUndefined() =>
        ValidPhone().WithPhoneType((PhoneType)(-1)).Error.ShouldBe(Phone.PhoneTypeInvalidMessage);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnUpdatedCopy_On_WithIsPrimary_WhenValueIsProvided(bool primary)
    {
        var original = ValidPhone();
        var updated = original.WithIsPrimary(primary);

        updated.IsPrimary.ShouldBe(primary);
        original.IsPrimary.ShouldBeFalse();
    }

    [Fact]
    public void UseValueEquality_WhenValuesAreTheSame()
    {
        var first = ValidPhone();
        var second = Phone.Create(first.Number, first.PhoneType, first.IsPrimary).Value;

        first.ShouldBe(second);
        first.ShouldNotBeSameAs(second);
    }

    private static Phone ValidPhone() => Phone.Create("555-123-4567", PhoneType.Mobile, false).Value;
}
