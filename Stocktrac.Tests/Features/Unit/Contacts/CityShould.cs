using Shouldly;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class CityShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenValueIsBlank(string? value)
    {
        var result = City.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(City.RequiredMessage);
    }

    [Fact]
    public void ReturnLengthError_On_Create_WhenTrimmedValueExceedsMaximum()
    {
        var result = City.Create($" {new string('a', City.MaximumLength + 1)} ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(City.InvalidLengthMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void PreserveTrimmedValue_On_Create_WhenValueIsAtLengthBoundary(int length)
    {
        var value = new string('a', length);

        var result = City.Create($"  {value}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value);
    }

    [Fact]
    public void PreserveInternalWhitespaceAndCasing_On_Create_WhenValueIsValid()
    {
        City.Create("  New  York  ").Value.Value.ShouldBe("New  York");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenValuesAreEqual()
    {
        var first = City.Create(" Albany ").Value;
        var second = City.Create("Albany").Value;
        var different = City.Create("albany").Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(different);
        default(City).Value.ShouldBeNull();
        default(City).ToString().ShouldBeEmpty();
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        City.MinimumLength.ShouldBe(1);
        City.MaximumLength.ShouldBe(100);
        City.RequiredMessage.ShouldBe("City is required.");
        City.InvalidLengthMessage.ShouldBe("City must be between 1 and 100 characters.");
    }
}
