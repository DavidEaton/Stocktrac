using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit.Contacts;

public class DriversLicenseNumberShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_Create_WhenNumberIsBlank(string? number)
    {
        var result = NonEmptyString.Create(number!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(NonEmptyString.RequiredMessage);
    }

    [Fact]
    public void ReturnLengthError_On_Create_WhenNumberExceedsMaximumLength()
    {
        var number = NonEmptyString.Create(new string('x', DriversLicenseNumber.MaximumLength + 1)).Value;

        var result = DriversLicenseNumber.Create(number);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicenseNumber.InvalidLengthMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(255)]
    public void PreserveNumber_On_Create_WhenNumberIsAtLengthBoundary(int length)
    {
        var number = new string('x', length);

        var result = Create(number);

        result.Number.Value.ShouldBe(number);
    }

    [Fact]
    public void PreserveWhitespaceAndCasing_On_Create_WhenNumberIsValid()
    {
        const string number = "Ab 12-cD";

        Create(number).Number.Value.ShouldBe(number);
    }

    [Fact]
    public void ReturnReplacement_On_ReplaceNumber_WhenNumberIsValid()
    {
        var replacement = NonEmptyString.Create("A123").Value;

        var result = DriversLicenseNumber.ReplaceNumber(replacement);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(replacement);
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenNumbersAreEqual()
    {
        var first = Create("A123");
        var second = Create("A123");
        var different = Create("a123");

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        (first == second).ShouldBeTrue();
        first.ShouldNotBe(different);
        (first != different).ShouldBeTrue();
    }

    [Fact]
    public void NotCreateAnInvalidObject_WhenDefaultInitialized()
    {
        DriversLicenseNumber? number = default;

        number.ShouldBeNull();
    }

    private static DriversLicenseNumber Create(string number) =>
        DriversLicenseNumber.Create(NonEmptyString.Create(number).Value).Value;
}
