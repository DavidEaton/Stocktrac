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
        ValidAddress().AddressFull.ShouldBe("123 Main St, Albany, NY 12345");
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
    public void ReturnUpdatedCopy_On_NewAddressLine1_WithoutChangingOtherValues()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);
        var replacement = AddressLine.Create("456 Oak Ave").Value;

        var result = original.NewAddressLine1(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine1.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine1));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_NewCity_WithoutChangingOtherValues()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);
        var replacement = City.Create("Buffalo").Value;

        var result = original.NewCity(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.City.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.City));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_NewState_WhenStateIsDefined()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);

        var result = original.NewState(State.TX);

        result.IsSuccess.ShouldBeTrue();
        result.Value.State.ShouldBe(State.TX);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.State));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void ReturnStateError_On_NewState_WhenStateIsUndefined(int state)
    {
        var original = ValidAddress();

        var result = original.NewState((State)state);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Address.StateInvalidMessage);
        original.State.ShouldBe(State.NY);
    }

    [Fact]
    public void ReturnUpdatedCopy_On_NewPostalCode_WithoutChangingOtherValues()
    {
        var original = ValidAddress(AddressLine.Create("Suite 1").Value);
        var replacement = PostalCode.Create("90210").Value;

        var result = original.NewPostalCode(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PostalCode.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.PostalCode));
    }

    [Fact]
    public void ReturnUpdatedCopy_On_NewAddressLine2_WhenValueIsPresent()
    {
        var original = ValidAddress();
        var replacement = AddressLine.Create("Suite 9").Value;

        var result = original.NewAddressLine2(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine2.ShouldBe(replacement);
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine2));
    }

    [Fact]
    public void ClearSecondLine_On_NewAddressLine2_WhenValueIsNull()
    {
        var original = ValidAddress(AddressLine.Create("Suite 9").Value);

        var result = original.NewAddressLine2(null);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine2.ShouldBeNull();
        AssertOnlyExpectedValueChanged(original, result.Value, nameof(Address.AddressLine2));
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenAllValuesAreEqual()
    {
        var first = ValidAddress();
        var second = ValidAddress();

        first.ShouldBe(second);
        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(first.NewState(State.TX).Value);
    }

    private static void AssertOnlyExpectedValueChanged(Address original, Address updated, string member)
    {
        if (member != nameof(Address.AddressLine1)) updated.AddressLine1.ShouldBe(original.AddressLine1);
        if (member != nameof(Address.AddressLine2)) updated.AddressLine2.ShouldBe(original.AddressLine2);
        if (member != nameof(Address.City)) updated.City.ShouldBe(original.City);
        if (member != nameof(Address.State)) updated.State.ShouldBe(original.State);
        if (member != nameof(Address.PostalCode)) updated.PostalCode.ShouldBe(original.PostalCode);
    }

    private static Address ValidAddress(AddressLine? line2 = null) =>
        Address.Create(ValidLine(), ValidCity(), State.NY, ValidPostalCode(), line2).Value;

    private static AddressLine ValidLine() => AddressLine.Create("123 Main St").Value;
    private static City ValidCity() => City.Create("Albany").Value;
    private static PostalCode ValidPostalCode() => PostalCode.Create("12345").Value;
}
