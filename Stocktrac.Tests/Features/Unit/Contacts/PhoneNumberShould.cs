using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class PhoneNumberShould
{
    [Theory]
    [InlineData(" 555-123-4567 ", "5551234567")]
    [InlineData(" +44 20 7946 0958 ", "+442079460958")]
    [InlineData("+61 (2) 9374-4000", "+61293744000")]
    public void StoreCanonicalValue_On_Create_WhenValueIsValid(string value, string canonicalValue)
    {
        var result = PhoneNumber.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(canonicalValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a phone")]
    [InlineData("12+345678")]
    [InlineData("+0123456789")]
    [InlineData("+1234567890123456")]
    public void ReturnInvalidError_On_Create_WhenValueIsInvalid(string? value)
    {
#pragma warning disable CS8604 // Testing the required-value boundary.
        var result = PhoneNumber.Create(value);
#pragma warning restore CS8604

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PhoneNumber.InvalidMessage);
    }

    [Theory]
    [InlineData("5551234", "555-1234")]
    [InlineData("555.123.4567", "(555) 123-4567")]
    [InlineData("+1 (555) 123-4567", "+15551234567")]
    public void ReturnFormattedValue_On_ToString(string value, string formatted) =>
        PhoneNumber.Create(value).Value.ToString().ShouldBe(formatted);

    [Fact]
    public void UseValueEquality_ForEquivalentInput()
    {
        var formatted = PhoneNumber.Create("555-123-4567").Value;
        var unformatted = PhoneNumber.Create("5551234567").Value;

        formatted.ShouldBe(unformatted);
    }
}
