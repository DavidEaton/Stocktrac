using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit;

public class VehicleShould
{
    [Fact]
    public void RepresentOptionalValuesAsAbsent_OnCreate()
    {
        var result = Vehicle.Create(
            Maybe<string>.None,
            Maybe<int>.None,
            "Trailer",
            string.Empty,
            Maybe<string>.None,
            Maybe<State>.None,
            Maybe<string>.None,
            Maybe<string>.None,
            nonTraditionalVehicle: true);

        result.IsSuccess.ShouldBeTrue();
        result.Value.VIN.HasNoValue.ShouldBeTrue();
        result.Value.Year.HasNoValue.ShouldBeTrue();
        result.Value.Plate.HasNoValue.ShouldBeTrue();
        result.Value.PlateStateProvince.HasNoValue.ShouldBeTrue();
        result.Value.UnitNumber.HasNoValue.ShouldBeTrue();
        result.Value.Color.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void SupportExplicitUpdateAndRemoveOperations()
    {
        var vehicle = CreateTraditionalVehicle();

        vehicle.UpdateYear(2020).IsSuccess.ShouldBeTrue();
        vehicle.UpdatePlate("ABC123").IsSuccess.ShouldBeTrue();
        vehicle.UpdatePlateStateProvince(State.NY).IsSuccess.ShouldBeTrue();
        vehicle.UpdateUnitNumber("42").IsSuccess.ShouldBeTrue();
        vehicle.UpdateColor("Blue").IsSuccess.ShouldBeTrue();

        vehicle.RemoveYear();
        vehicle.RemovePlate();
        vehicle.RemovePlateStateProvince();
        vehicle.RemoveUnitNumber();
        vehicle.RemoveColor();

        vehicle.Year.HasNoValue.ShouldBeTrue();
        vehicle.Plate.HasNoValue.ShouldBeTrue();
        vehicle.PlateStateProvince.HasNoValue.ShouldBeTrue();
        vehicle.UnitNumber.HasNoValue.ShouldBeTrue();
        vehicle.Color.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void PreserveMode_WhenTransitionWouldBreakVinInvariant()
    {
        var vehicle = Vehicle.Create(
            Maybe<string>.None,
            Maybe<int>.None,
            "Trailer",
            string.Empty,
            Maybe<string>.None,
            Maybe<State>.None,
            Maybe<string>.None,
            Maybe<string>.None,
            nonTraditionalVehicle: true).Value;

        var result = vehicle.UpdateNonTraditionalVehicle(false);

        result.IsFailure.ShouldBeTrue();
        vehicle.NonTraditionalVehicle.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnFailureAndPreserveAbsence_On_UpdatePlate_WhenValueIsMissing(string? value)
    {
        var vehicle = CreateTraditionalVehicle();

        var result = vehicle.UpdatePlate(value!);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Vehicle.OptionalTextRequiredMessage);
        vehicle.Plate.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenPresentColorIsMissing()
    {
        var result = Vehicle.Create(
            "1HGCM82633A004352",
            Maybe<int>.None,
            "Honda",
            "Accord",
            Maybe<string>.None,
            Maybe<State>.None,
            Maybe<string>.None,
            Maybe<string>.From("   "));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain(Vehicle.OptionalTextRequiredMessage);
    }

    private static Vehicle CreateTraditionalVehicle() =>
        Vehicle.Create(
            "1HGCM82633A004352",
            Maybe<int>.None,
            "Honda",
            "Accord",
            Maybe<string>.None,
            Maybe<State>.None,
            Maybe<string>.None,
            Maybe<string>.None).Value;
}
