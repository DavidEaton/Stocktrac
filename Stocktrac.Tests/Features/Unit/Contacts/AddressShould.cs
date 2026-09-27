using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class AddressShould
{
    [Fact]
    public void ContainNoValue_On_Default()
    {
        Address.Default.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void ExposeAllValues_On_Create_WhenValuesAreValid()
    {
        var line1 = ValidLine();
        var line2 = CreateLine("Apt 4");
        var city = ValidCity();
        var postalCode = ValidPostalCode();

        var result = Address.Create(line1, city, State.NY, postalCode, line2);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine1.ShouldBe(line1);
        result.Value.AddressLine2.ShouldBe(line2);
        result.Value.City.ShouldBe(city);
        result.Value.State.ShouldBe(State.NY);
        result.Value.PostalCode.ShouldBe(postalCode);
        result.Value.AddressFull.ShouldBe("123 Main St, Apt 4, Albany, NY 12345");
        result.Value.ToString().ShouldBe(result.Value.AddressFull);
    }

    [Fact]
    public void OmitSecondLineAndItsSeparator_On_AddressFull_WhenSecondLineIsAbsent()
    {
        ValidAddress(Maybe<AddressLine>.None)
            .AddressFull.ShouldBe("123 Main St, Albany, NY 12345");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    [InlineData(2147483647)]
    public void ReturnStateError_On_Create_WhenStateIsUndefined(int state)
    {
        var result = Address.Create(ValidLine(), ValidCity(), (State)state, ValidPostalCode());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("A valid State is required.");
    }

    [Fact]
    public void ReturnEveryError_On_Create_WhenSeveralComponentsAreInvalid()
    {
        var result = Address.Create(null!, null!, (State)(-1), null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain(NonEmptyString.RequiredMessage);
        result.Error.ShouldContain("A valid State is required.");
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceAddressLine1_WithoutChangingOtherValues()
    {
        var original = ValidAddress(CreateLine("Suite 1"));
        var replacement = CreateLine("456 Oak Ave");

        var result = original.ReplaceAddressLine1(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine1.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine1));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceCity_WithoutChangingOtherValues()
    {
        var original = ValidAddress(CreateLine("Suite 1"));
        var replacement = CreateCity("Buffalo");

        var result = original.ReplaceCity(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.City.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.City));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplaceState_WhenStateIsDefined()
    {
        var original = ValidAddress(CreateLine("Suite 1"));

        var result = original.ReplaceState(State.TX);

        result.IsSuccess.ShouldBeTrue();
        result.Value.State.ShouldBe(State.TX);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.State));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void ReturnStateError_On_ReplaceState_WhenStateIsUndefined(int state)
    {
        var original = ValidAddress(Maybe<AddressLine>.None);

        var result = original.ReplaceState((State)state);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("A valid State is required.");
        original.State.ShouldBe(State.NY);
    }

    [Fact]
    public void ReturnUpdatedCopy_On_ReplacePostalCode_WithoutChangingOtherValues()
    {
        var original = ValidAddress(CreateLine("Suite 1"));
        var replacement = CreatePostalCode("90210");

        var result = original.ReplacePostalCode(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PostalCode.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.PostalCode));

        NonEmptyString moops = default;
        moops.ShouldBeSameAs(default(NonEmptyString));

    }

    [Fact]
    public void ReturnUpdatedCopy_On_AddOrReplaceAddressLine2_WhenValueIsPresent()
    {
        var original = ValidAddress(Maybe<AddressLine>.None);
        var replacement = CreateLine("Suite 9");

        var result = original.AddOrReplaceAddressLine2(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine2.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine2));
    }

    [Fact]
    public void ReturnAbsentAddressLine2_On_RemoveAddressLine2()
    {
        var original = ValidAddress(CreateLine("Suite 9"));

        var edited = original.RemoveAddressLine2();

        edited.AddressLine2.HasNoValue.ShouldBeTrue();
        AssertOnlyExpectedValueChanged(original, edited, nameof(Address.AddressLine2));
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenAllValuesAreEqual()
    {
        var first = ValidAddress(Maybe<AddressLine>.None);
        var second = ValidAddress(Maybe<AddressLine>.None);

        first.ShouldBe(second);
        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(first.ReplaceState(State.TX).Value);
    }

    private static void AssertOnlyExpectedValueChanged(Address original, Address updated, string member)
    {
        if (member != nameof(Address.AddressLine1)) updated.AddressLine1.ShouldBe(original.AddressLine1);
        if (member != nameof(Address.AddressLine2)) updated.AddressLine2.ShouldBe(original.AddressLine2);
        if (member != nameof(Address.City)) updated.City.ShouldBe(original.City);
        if (member != nameof(Address.State)) updated.State.ShouldBe(original.State);
        if (member != nameof(Address.PostalCode)) updated.PostalCode.ShouldBe(original.PostalCode);
    }

    private static Address ValidAddress(Maybe<AddressLine> line2) =>
        Address.Create(ValidLine(), ValidCity(), State.NY, ValidPostalCode(), line2).Value;
    private static AddressLine ValidLine() => CreateLine("123 Main St");
    private static City ValidCity() => CreateCity("Albany");
    private static PostalCode ValidPostalCode() => CreatePostalCode("12345");
    private static AddressLine CreateLine(string value) => AddressLine.Create(NonEmptyString.Create(value).Value).Value;
    private static City CreateCity(string value) => City.Create(NonEmptyString.Create(value).Value).Value;
    private static PostalCode CreatePostalCode(string value) => PostalCode.Create(NonEmptyString.Create(value).Value).Value;
}
