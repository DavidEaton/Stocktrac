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

    [Fact]
    public void ApplySameValidation_On_NewNumber_WhenNumberIsProvided()
    {
        DriversLicenseNumber.NewNumber(null!).Error.ShouldBe(DriversLicenseNumber.RequiredMessage);
        DriversLicenseNumber.NewNumber("   ").Error.ShouldBe(DriversLicenseNumber.RequiredMessage);
        DriversLicenseNumber.NewNumber("ab").Error.ShouldBe(DriversLicenseNumber.InvalidLengthMessage);
        DriversLicenseNumber.NewNumber(new string('x', 256)).Error
            .ShouldBe(DriversLicenseNumber.InvalidLengthMessage);
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
    }
}