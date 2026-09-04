using Shouldly;
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
        var result = DriversLicenseNumber.Create(number);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicenseNumber.RequiredMessage);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(256)]
    public void ReturnLengthError_On_Create_WhenTrimmedNumberIsOutsideBounds(int length)
    {
        var result = DriversLicenseNumber.Create($"  {new string('x', length)}  ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicenseNumber.InvalidLengthMessage);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(255)]
    public void PreserveTrimmedNumber_On_Create_WhenNumberIsAtLengthBoundary(int length)
    {
        var number = new string('x', length);

        var result = DriversLicenseNumber.Create($"  {number}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(number);
    }

    [Fact]
    public void PreserveInternalWhitespaceAndCasing_On_Create_WhenNumberIsValid()
    {
        var result = DriversLicenseNumber.Create("  Ab 12-cD  ");

        result.Value.Number.ShouldBe("Ab 12-cD");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnRequiredError_On_NewNumber_WhenNumberIsBlank(string? number)
    {
        var result = DriversLicenseNumber.NewNumber(number!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicenseNumber.RequiredMessage);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(256)]
    public void ReturnLengthError_On_NewNumber_WhenTrimmedNumberIsOutsideBounds(int length)
    {
        var result = DriversLicenseNumber.NewNumber($"  {new string('x', length)}  ");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DriversLicenseNumber.InvalidLengthMessage);
    }

    [Fact]
    public void PreserveTrimmedNumber_On_NewNumber_WhenNumberIsValid()
    {
        DriversLicenseNumber.NewNumber("  A123  ").Value.Number.ShouldBe("A123");
    }

    [Fact]
    public void BeEqualAndHaveMatchingHashCodes_WhenNumbersAreEqual()
    {
        var first = DriversLicenseNumber.Create("A123").Value;
        var second = DriversLicenseNumber.Create("A123").Value;
        var different = DriversLicenseNumber.Create("a123").Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        (first == second).ShouldBeTrue();
        first.ShouldNotBe(different);
        (first != different).ShouldBeTrue();
        default(DriversLicenseNumber).Number.ShouldBeNull();
    }

    [Fact]
    public void ExposeValidationContractConstants()
    {
        DriversLicenseNumber.MinimumLength.ShouldBe(3);
        DriversLicenseNumber.MaximumLength.ShouldBe(255);
        DriversLicenseNumber.RequiredMessage.ShouldBe("Drivers License Number is required.");
        DriversLicenseNumber.InvalidLengthMessage.ShouldBe("Value must be between 3 and 255 characters.");
    }
}
