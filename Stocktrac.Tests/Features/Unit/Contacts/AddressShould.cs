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
    public void NormalizeAndExposeAllValues_On_Create_WhenValuesAreValid()
    {
        var result = Address.Create(" 123 Main St ", " Albany ", State.NY, " 12345 ", " Apt 4 ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.AddressLine1.ShouldBe("123 Main St");
        result.Value.AddressLine2.ShouldBe("Apt 4");
        result.Value.City.ShouldBe("Albany");
        result.Value.State.ShouldBe(State.NY);
        result.Value.PostalCode.ShouldBe("12345");
        result.Value.AddressFull.ShouldBe("123 Main St, Apt 4, Albany, NY 12345");
        result.Value.ToString().ShouldBe(result.Value.AddressFull);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenAddressLine1IsBlank(string? value) =>
        Address.Create(value!, "City", State.AL, "12345").Error.ShouldBe(Address.AddressRequiredMessage);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenCityIsBlank(string? value) =>
        Address.Create("123 Main", value!, State.AL, "12345").Error.ShouldBe(Address.CityRequiredMessage);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenPostalCodeIsBlank(string? value) =>
        Address.Create("123 Main", "City", State.AL, value!).Error.ShouldBe(Address.PostalCodeRequiredMessage);

    [Fact]
    public void ReturnStateError_On_Create_WhenStateIsUndefined() =>
        Address.Create("123 Main", "City", (State)int.MaxValue, "12345").Error.ShouldBe(Address.StateInvalidMessage);

    [Theory]
    [InlineData("12")]
    [InlineData("1234567890")]
    [InlineData("12A45")]
    public void ReturnPostalCodeError_On_Create_WhenPostalCodeIsInvalid(string value) =>
        Address.Create("123 Main", "City", State.AL, value).Error.ShouldBe(Address.PostalCodeInvalidMessage);

    [Theory]
    [InlineData(2)]
    [InlineData(256)]
    public void ReturnAddressLengthError_On_Create_WhenAddressLine1IsOutsideBounds(int length) =>
        Address.Create(new string('a', length), "City", State.AL, "12345").Error.ShouldBe(Address.AddressLengthMessage);

    [Fact]
    public void ReturnAddressLengthError_On_Create_WhenAddressLine2ExceedsMaximum() =>
        Address.Create("123 Main", "City", State.AL, "12345", new string('a', 256)).Error.ShouldBe(Address.AddressLengthMessage);

    [Theory]
    [InlineData(2)]
    [InlineData(256)]
    public void ReturnCityLengthError_On_Create_WhenCityIsOutsideBounds(int length) =>
        Address.Create("123 Main", new string('a', length), State.AL, "12345").Error.ShouldBe(Address.CityLengthMessage);

    [Theory]
    [InlineData(3, 5)]
    [InlineData(255, 9)]
    public void AcceptExactLengthBoundaries_On_Create_WhenValuesAreValid(int textLength, int postalLength)
    {
        var result = Address.Create(new string('a', textLength), new string('c', textLength), State.AL,
            new string('1', postalLength), new string('b', textLength));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void OmitSecondLinePunctuation_On_AddressFull_WhenSecondLineIsAbsent()
    {
        var address = Address.Create("123 Main", "Albany", State.NY, "12345", "  ").Value;

        address.AddressLine2.ShouldBeNull();
        address.AddressFull.ShouldBe("123 Main Albany, NY 12345");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(256)]
    public void ReturnLengthError_On_NewAddressLine1_WhenValueIsOutsideBounds(int length)
    {
        var original = ValidAddress();
        var result = original.NewAddressLine1(new string('a', length));

        result.Error.ShouldBe(Address.AddressLengthMessage);
        original.AddressLine1.ShouldBe("123 Main");
    }

    [Fact]
    public void ReturnNormalizedCopy_On_NewAddressLine1_WhenValueIsValid()
    {
        var original = ValidAddress();
        var updated = original.NewAddressLine1(" 456 Oak ").Value;

        updated.AddressLine1.ShouldBe("456 Oak");
        original.AddressLine1.ShouldBe("123 Main");
    }

    [Theory]
    [InlineData(null, "City is required")]
    [InlineData("  ", "City is required")]
    [InlineData("ab", "City must be between 3 and 255 character(s) in length")]
    public void ReturnSpecificError_On_NewCity_WhenValueIsInvalid(string? city, string error)
    {
        var original = ValidAddress();
        original.NewCity(city!).Error.ShouldBe(error);
        original.City.ShouldBe("City");
    }

    [Fact]
    public void ReturnNormalizedCopy_On_NewCity_WhenValueIsValid() =>
        ValidAddress().NewCity(" Albany ").Value.City.ShouldBe("Albany");

    [Fact]
    public void ReturnError_On_NewState_WhenStateIsUndefined() =>
        ValidAddress().NewState((State)(-1)).Error.ShouldBe(Address.StateInvalidMessage);

    [Fact]
    public void ReturnCopy_On_NewState_WhenStateIsDefined() =>
        ValidAddress().NewState(State.TX).Value.State.ShouldBe(State.TX);

    [Theory]
    [InlineData(null, "Postal Code is required")]
    [InlineData("  ", "Postal Code is required")]
    [InlineData("1234", "Please enter a valid Postal Code")]
    [InlineData("1234A", "Please enter a valid Postal Code")]
    public void ReturnSpecificError_On_NewPostalCode_WhenValueIsInvalid(string? code, string error) =>
        ValidAddress().NewPostalCode(code!).Error.ShouldBe(error);

    [Fact]
    public void ReturnNormalizedCopy_On_NewPostalCode_WhenValueIsValid() =>
        ValidAddress().NewPostalCode(" 987654321 ").Value.PostalCode.ShouldBe("987654321");

    [Fact]
    public void ClearSecondLine_On_NewAddressLine2_WhenValueIsBlank() =>
        ValidAddress().NewAddressLine2("  ").Value.AddressLine2.ShouldBeNull();

    [Fact]
    public void ReturnNormalizedCopy_On_NewAddressLine2_WhenValueIsValid() =>
        ValidAddress().NewAddressLine2(" Suite 9 ").Value.AddressLine2.ShouldBe("Suite 9");

    [Fact]
    public void ReturnLengthError_On_NewAddressLine2_WhenValueExceedsMaximum() =>
        ValidAddress().NewAddressLine2(new string('x', 256)).Error.ShouldBe(Address.AddressLengthMessage);

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenAllValuesAreEqual()
    {
        var first = ValidAddress();
        var second = ValidAddress();

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(first.NewState(State.TX).Value);
    }

    private static Address ValidAddress() => Address.Create("123 Main", "City", State.AL, "12345").Value;
}