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
        result.Value.Number.ShouldBe("555-123-4567");
        result.Value.PhoneType.ShouldBe(PhoneType.Mobile);
        result.Value.IsPrimary.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a phone")]
    public void ReturnInvalidError_On_Create_WhenNumberIsInvalid(string? number) =>
        Phone.Create(number!, PhoneType.Home, false).Error.ShouldBe(Phone.InvalidMessage);

    [Fact]
    public void ReturnPhoneTypeError_On_Create_WhenPhoneTypeIsUndefined() =>
        Phone.Create("5551234567", (PhoneType)99, false).Error.ShouldBe(Phone.PhoneTypeInvalidMessage);

    [Theory]
    [InlineData("5551234", "555-1234")]
    [InlineData("555.123.4567", "(555) 123-4567")]
    [InlineData("+1 (555) 123-4567", "15551234567")]
    public void ReturnNumericFormattedValue_On_ToString_WhenNumberIsValid(string number, string formatted) =>
        Phone.Create(number, PhoneType.Home, false).Value.ToString().ShouldBe(formatted);

    [Fact]
    public void ReturnUpdatedCopy_On_SetNumber_WhenNumberIsValid()
    {
        var original = ValidPhone();
        var updated = original.SetNumber(" 555-987-6543 ").Value;

        updated.Number.ShouldBe("555-987-6543");
        updated.PhoneType.ShouldBe(original.PhoneType);
        updated.IsPrimary.ShouldBe(original.IsPrimary);
        original.Number.ShouldBe("555-123-4567");
    }

    [Fact]
    public void ReturnErrorAndLeaveOriginalUnchanged_On_SetNumber_WhenNumberIsInvalid()
    {
        var original = ValidPhone();
        original.SetNumber("invalid").Error.ShouldBe(Phone.InvalidMessage);
        original.Number.ShouldBe("555-123-4567");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_SetPhoneType_WhenPhoneTypeIsDefined() =>
        ValidPhone().SetPhoneType(PhoneType.Work).Value.PhoneType.ShouldBe(PhoneType.Work);

    [Fact]
    public void ReturnError_On_SetPhoneType_WhenPhoneTypeIsUndefined() =>
        ValidPhone().SetPhoneType((PhoneType)(-1)).Error.ShouldBe(Phone.PhoneTypeInvalidMessage);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnUpdatedCopy_On_SetIsPrimary_WhenValueIsProvided(bool primary)
    {
        var original = ValidPhone();
        var updated = original.SetIsPrimary(primary).Value;

        updated.IsPrimary.ShouldBe(primary);
        original.IsPrimary.ShouldBeFalse();
    }

    [Fact]
    public void UseEntityIdentitySemantics_WhenInstancesAreTransient()
    {
        var first = ValidPhone();
        var second = Phone.Create(first.Number, first.PhoneType, first.IsPrimary).Value;

        first.ShouldNotBe(second);
        first.ShouldBeSameAs(first);
    }

    private static Phone ValidPhone() => Phone.Create("555-123-4567", PhoneType.Mobile, false).Value;
}