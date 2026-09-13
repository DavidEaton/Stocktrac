using CSharpFunctionalExtensions;
using Shouldly;
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
        var line1 = AddressLine.Create("123 Main St").Value;
        var line2 = AddressLine.Create("Apt 4").Value;
        var city = City.Create("Albany").Value;
        var postalCode = PostalCode.Create("12345").Value;

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
        result.Error.ShouldBe(Address.StateInvalidMessage);
    }

    [Fact]
    public void ReturnEveryError_On_Create_WhenSeveralComponentsAreInvalid()
    {
        var result = Address.Create(null!, null!, (State)(-1), null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(string.Join(
            Environment.NewLine,
            Address.AddressRequiredMessage,
            Address.CityRequiredMessage,
            Address.StateInvalidMessage,
            Address.PostalCodeRequiredMessage));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithAddressLine1_WithoutChangingOtherValues()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);
        var replacement = AddressLine.Create("456 Oak Ave").Value;

        var result = original.WithAddressLine1(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine1.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine1));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithCity_WithoutChangingOtherValues()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);
        var replacement = City.Create("Buffalo").Value;

        var result = original.WithCity(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.City.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.City));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithState_WhenStateIsDefined()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);

        var result = original.WithState(State.TX);

        result.IsSuccess.ShouldBeTrue();
        result.Value.State.ShouldBe(State.TX);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.State));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void ReturnStateError_On_WithState_WhenStateIsUndefined(int state)
    {
        var original = ValidAddress(Maybe<AddressLine>.None);

        var result = original.WithState((State)state);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Address.StateInvalidMessage);
        original.State.ShouldBe(State.NY);
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithPostalCode_WithoutChangingOtherValues()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);
        var replacement = PostalCode.Create("90210").Value;

        var result = original.WithPostalCode(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PostalCode.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.PostalCode));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_WithAddressLine2_WhenValueIsPresent()
    {
        var original = ValidAddress(Maybe<AddressLine>.None);
        var replacement = AddressLine.Create("Suite 9").Value;

        var result = original.WithAddressLine2(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine2.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine2));
    }

    [Fact]
    public void ReturnAbsentAddressLine2_On_WithoutAddressLine2()
    {
        var original = ValidAddress(AddressLine.Create("Suite 9").Value);

        var edited = original.WithoutAddressLine2();

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
        first.ShouldNotBe(first.WithState(State.TX).Value);
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
    private static AddressLine ValidLine() => AddressLine.Create("123 Main St").Value;
    private static City ValidCity() => City.Create("Albany").Value;
    private static PostalCode ValidPostalCode() => PostalCode.Create("12345").Value;
}
