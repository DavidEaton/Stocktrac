using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;

namespace Stocktrac.Tests.Features.Unit;

public class NonTraditionalVehicleKindShould
{
    [Fact]
    public void ReturnFirstError_On_Create_WhenVinAndDescriptionAreInvalid()
    {
        var result = NonTraditionalVehicleKind.Create("invalid", Maybe<string>.None, Maybe<string>.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Vehicle.InvalidVinMessage);
    }

    [Fact]
    public void NormalizePresentValues_On_Create()
    {
        var result = NonTraditionalVehicleKind.Create(" 1HGCM82633A004352 ", " Trailer ", " Utility ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.VIN.Value.ShouldBe("1HGCM82633A004352");
        result.Value.Make.Value.ShouldBe("Trailer");
        result.Value.Model.Value.ShouldBe("Utility");
    }

    [Fact]
    public void PermitAbsentVinAndMake_On_Create_WhenModelIsPresent()
    {
        var result = NonTraditionalVehicleKind.Create(Maybe<string>.None, Maybe<string>.None, "Trailer");

        result.IsSuccess.ShouldBeTrue();
        result.Value.VIN.HasNoValue.ShouldBeTrue();
        result.Value.Make.HasNoValue.ShouldBeTrue();
        result.Value.Model.Value.ShouldBe("Trailer");
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenMakeAndModelAreAbsent() =>
        NonTraditionalVehicleKind.Create(Maybe<string>.None, Maybe<string>.None, Maybe<string>.None)
            .Error.ShouldBe(Vehicle.NonTraditionalVehicleInvalidMakeModelMessage);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    public void ReturnFailure_On_Create_WhenPresentVinIsInvalid(string vin) =>
        NonTraditionalVehicleKind.Create(vin, "Trailer", Maybe<string>.None)
            .Error.ShouldBe(Vehicle.InvalidVinMessage);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnFailure_On_Create_WhenPresentMakeIsBlank(string make) =>
        NonTraditionalVehicleKind.Create(Maybe<string>.None, make, "Utility")
            .Error.ShouldBe(Vehicle.NonTraditionalVehicleInvalidMakeModelMessage);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnFailure_On_Create_WhenPresentModelIsBlank(string model) =>
        NonTraditionalVehicleKind.Create(Maybe<string>.None, "Trailer", model)
            .Error.ShouldBe(Vehicle.NonTraditionalVehicleInvalidMakeModelMessage);

    [Theory]
    [InlineData(1)]
    [InlineData(51)]
    public void PreserveFlexibleDescriptionLength_On_Create(int length)
    {
        var description = new string('a', length);

        NonTraditionalVehicleKind.Create(Maybe<string>.None, description, Maybe<string>.None)
            .Value.Make.Value.ShouldBe(description);
    }
}
