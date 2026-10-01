using Stocktrac.Domain.Features;
using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class ContactPhoneShould
{
    [Fact]
    public void NormalizeAndPreserveValues_On_Create_WhenValuesAreValid()
    {
        var result = Create(" 555-123-4567 ", PhoneType.Mobile, true);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.Value.Value.ShouldBe("5551234567");
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
        var result = Create(number, PhoneType.Mobile, false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.Value.Value.ShouldBe(canonicalNumber);
        result.Value.ToString().ShouldBe(canonicalNumber);
    }

    [Fact]
    public void ReturnInvalidError_OnCreate_WhenNumberHasInvalidFormat()
    {
        var result = Create(
            "not a phone",
            PhoneType.Home,
            false);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PhoneNumber.InvalidMessage);
    }

    [Theory]
    [InlineData("12+345678")]
    [InlineData("+0123456789")]
    [InlineData("+1234567890123456")]
    public void ReturnInvalidError_On_Create_WhenInternationalNumberIsNotCanonicalizable(string number) =>
        Create(number, PhoneType.Home, false).Error.ShouldBe(PhoneNumber.InvalidMessage);

    [Fact]
    public void ReturnPhoneTypeError_On_Create_WhenPhoneTypeIsUndefined() =>
        Create("5551234567", (PhoneType)99, false).Error.ShouldBe(ContactPhone.PhoneTypeInvalidMessage);

    [Theory]
    [InlineData("5551234", "555-1234")]
    [InlineData("555.123.4567", "(555) 123-4567")]
    [InlineData("+1 (555) 123-4567", "+15551234567")]
    public void ReturnNumericFormattedValue_On_ToString_WhenNumberIsValid(string number, string formatted) =>
        Create(number, PhoneType.Home, false).Value.ToString().ShouldBe(formatted);

    [Fact]
    public void ReplaceOnlyNumber_On_ReplaceNumber_WhenNumberIsValid()
    {
        var original = ValidPhone();
        var updated = ReplaceNumber(original, " 555-987-6543 ").Value;

        updated.Number.Value.Value.ShouldBe("5559876543");
        updated.PhoneType.ShouldBe(original.PhoneType);
        updated.IsPrimary.ShouldBe(original.IsPrimary);
        original.Number.Value.Value.ShouldBe("5551234567");
    }

    [Fact]
    public void StoreCanonicalInternationalNumber_On_ReplaceNumber_WhenNumberHasInternationalFormat()
    {
        var updated = ReplaceNumber(ValidPhone(), "+33 (1) 42 68 53 00").Value;

        updated.Number.Value.Value.ShouldBe("+33142685300");
    }

    [Fact]
    public void StoreSameRepresentation_On_CreateAndReplaceNumber()
    {
        const string formattedNumber = " +1 (555) 123-4567 ";

        var created = Create(formattedNumber, PhoneType.Mobile, false).Value;
        var replaced = ReplaceNumber(ValidPhone(), formattedNumber).Value;

        created.Number.ShouldBe(replaced.Number);
        created.Number.Value.Value.ShouldBe("+15551234567");
    }

    [Fact]
    public void ReturnErrorAndLeaveOriginalUnchanged_On_ReplaceNumber_WhenNumberIsInvalid()
    {
        var original = ValidPhone();
        ReplaceNumber(original, "invalid").Error.ShouldBe(PhoneNumber.InvalidMessage);
        original.Number.Value.Value.ShouldBe("5551234567");
    }

    [Fact]
    public void ReplacePhoneType_On_ReplacePhoneType_WhenPhoneTypeIsDefined() =>
        ValidPhone().ReplacePhoneType(PhoneType.Work).Value.PhoneType.ShouldBe(PhoneType.Work);

    [Fact]
    public void ReturnError_On_ReplacePhoneType_WhenPhoneTypeIsUndefined() =>
        ValidPhone().ReplacePhoneType((PhoneType)(-1)).Error.ShouldBe(ContactPhone.PhoneTypeInvalidMessage);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReplacePrimaryStatus_On_ReplaceIsPrimary_WhenValueIsProvided(bool primary)
    {
        var original = ValidPhone();
        var result = original.ReplaceIsPrimary(primary);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsPrimary.ShouldBe(primary);
        original.IsPrimary.ShouldBeFalse();
    }

    [Fact]
    public void UseValueEquality_WhenValuesAreTheSame()
    {
        var first = ValidPhone();
        var second = Create(first.Number.Value.Value, first.PhoneType, first.IsPrimary).Value;

        first.ShouldBe(second);
        first.ShouldNotBeSameAs(second);
    }

    private static Result<ContactPhone> Create(string number, PhoneType phoneType, bool isPrimary) =>
        PhoneNumber.Create(NonEmptyString.Create(number).Value)
            .Bind(validNumber => ContactPhone.Create(validNumber, phoneType, isPrimary));

    private static Result<ContactPhone> ReplaceNumber(ContactPhone phone, string number) =>
        PhoneNumber.Create(NonEmptyString.Create(number).Value)
            .Bind(validNumber => phone.ReplaceNumber(validNumber));

    private static ContactPhone ValidPhone() => Create("555-123-4567", PhoneType.Mobile, false).Value;
}
