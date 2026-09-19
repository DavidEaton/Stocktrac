using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class AddressLineShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void ReturnRequiredError_On_Create_WhenValueIsBlank(string? value)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        var result = AddressLine.Create(value);
#pragma warning restore CS8604 // Possible null reference argument.

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain(AddressLine.RequiredMessage);
    }

    [Theory]
    [InlineData(0)]
    public void ReturnRequiredError_On_Create_WhenTrimmedValueIsOutsideLowerBound(int length)
    {
        var result = AddressLine.Create($"  {new string('a', length)}  ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AddressLine.RequiredMessage);
    }
    [Theory]
    [InlineData(256)]
    public void ReturnLengthError_On_Create_WhenTrimmedValueIsOutsideUpperBound(int length)
    {
        var result = AddressLine.Create($"  {new string('a', length)}  ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AddressLine.InvalidLengthMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(255)]
    public void PreserveTrimmedValue_On_Create_WhenValueIsAtLengthBoundary(int length)
    {
        var value = new string('a', length);

        var result = AddressLine.Create($"  {value}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value);
    }

    [Fact]
    public void PreserveInternalWhitespaceAndCasing_On_Create_WhenValueIsValid()
    {
        AddressLine.Create("  12-B Main  Street  ").Value.Value.ShouldBe("12-B Main  Street");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = AddressLine.Create(" Main ").Value;
        var second = AddressLine.Create("Main").Value;
        var different = AddressLine.Create("main").Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        (first == second).ShouldBeTrue();
        first.ShouldNotBe(different);
    }

    [Fact]
    public void NotCreateAnInvalidObject_WhenDefaultInitialized()
    {
        AddressLine? addressLine = default;

        addressLine.ShouldBeNull();
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        AddressLine.MinimumLength.ShouldBe(1);
        AddressLine.MaximumLength.ShouldBe(255);
        AddressLine.RequiredMessage.ShouldBe("Address Line is required.");
        AddressLine.InvalidLengthMessage.ShouldBe("Address must be between 1 and 255 characters.");
    }
}
