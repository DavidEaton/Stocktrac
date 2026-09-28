using Stocktrac.Domain.Features;
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
        var result = PhoneNumber.Create(NonEmptyString.Create(value).Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.Value.ShouldBe(canonicalValue);
    }

    [Theory]
    [InlineData("not a phone")]
    [InlineData("12+345678")]
    [InlineData("+0123456789")]
    [InlineData("+1234567890123456")]
    public void ReturnInvalidError_On_Create_WhenValueIsInvalid(string value)
    {
        var result = PhoneNumber.Create(NonEmptyString.Create(value).Value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PhoneNumber.InvalidMessage);
    }

    [Theory]
    [InlineData("5551234", "555-1234")]
    [InlineData("555.123.4567", "(555) 123-4567")]
    [InlineData("+1 (555) 123-4567", "+15551234567")]
    public void ReturnFormattedValue_On_ToString(string value, string formatted) =>
        PhoneNumber.Create(NonEmptyString.Create(value).Value).Value.ToString().ShouldBe(formatted);

    [Fact]
    public void UseValueEquality_ForEquivalentInput()
    {
        var formatted = PhoneNumber.Create(NonEmptyString.Create("555-123-4567").Value).Value;
        var unformatted = PhoneNumber.Create(NonEmptyString.Create("5551234567").Value).Value;

        formatted.ShouldBe(unformatted);
    }
}
