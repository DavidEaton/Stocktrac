using CSharpFunctionalExtensions;
using Shouldly;
using Stocktrac.Domain.Features;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Tests.Features.Unit;

public class VehicleShould
{
    private static readonly DateOnly Today = new(2025, 6, 15);

    [Fact]
    public void ReturnFirstError_On_Create_WhenKindAndRegistrationAreInvalid()
    {
        var result = Vehicle.Create(default, 0, "   ", (State)(-1), "   ", "   ", Today);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Vehicle.KindRequiredMessage);
    }

    [Fact]
    public void RepresentOptionalValuesAsAbsent_On_Create_WhenKindIsNonTraditional()
    {
        var vehicle = CreateVehicle(NonTraditionalVehicleKind.Create(
            Maybe<string>.None, "Trailer", Maybe<string>.None).Value).Value;

        vehicle.VIN.HasNoValue.ShouldBeTrue();
        vehicle.Model.HasNoValue.ShouldBeTrue();
        vehicle.Year.HasNoValue.ShouldBeTrue();
        vehicle.Plate.HasNoValue.ShouldBeTrue();
        vehicle.PlateStateProvince.HasNoValue.ShouldBeTrue();
        vehicle.UnitNumber.HasNoValue.ShouldBeTrue();
        vehicle.Color.HasNoValue.ShouldBeTrue();
        (vehicle.Kind is NonTraditionalVehicleKind).ShouldBeTrue();
    }

    [Fact]
    public void ClearOptionalValues_On_RemoveOperations_WhenValuesExist()
    {
        var vehicle = CreateTraditionalVehicle();
        vehicle.UpdateYear(2020, Today).IsSuccess.ShouldBeTrue();
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
    public void PreserveSharedState_On_ReplaceKind_WhenTransitioningBetweenKinds()
    {
        var vehicle = CreateTraditionalVehicle();
        vehicle.UpdateYear(2020, Today);
        vehicle.UpdatePlate("ABC123");
        vehicle.UpdatePlateStateProvince(State.NY);
        vehicle.UpdateUnitNumber("42");
        vehicle.UpdateColor("Blue");
        vehicle.UpdateActive(false);
        var originalId = vehicle.Id;
        VehicleKind trailer = NonTraditionalVehicleKind.Create(
            Maybe<string>.None, "Trailer", Maybe<string>.None).Value;

        vehicle.ReplaceKind(trailer).IsSuccess.ShouldBeTrue();
        (vehicle.Kind is NonTraditionalVehicleKind).ShouldBeTrue();
        vehicle.VIN.HasNoValue.ShouldBeTrue();
        vehicle.Make.Value.ShouldBe("Trailer");
        vehicle.Model.HasNoValue.ShouldBeTrue();

        VehicleKind traditional = TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord").Value;
        vehicle.ReplaceKind(traditional).IsSuccess.ShouldBeTrue();
        (vehicle.Kind is TraditionalVehicleKind).ShouldBeTrue();
        vehicle.VIN.Value.ShouldBe("1HGCM82633A004352");
        vehicle.Id.ShouldBe(originalId);
        vehicle.Year.Value.ShouldBe(2020);
        vehicle.Plate.Value.ShouldBe("ABC123");
        vehicle.PlateStateProvince.Value.ShouldBe(State.NY);
        vehicle.UnitNumber.Value.ShouldBe("42");
        vehicle.Color.Value.ShouldBe("Blue");
        vehicle.Active.ShouldBeFalse();
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenKindIsDefault()
    {
        var result = CreateVehicle(default);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Vehicle.KindRequiredMessage);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnFailure_On_Create_WhenCaseIsNull(bool traditional)
    {
        VehicleKind kind = traditional
            ? new VehicleKind((TraditionalVehicleKind)null!)
            : new VehicleKind((NonTraditionalVehicleKind)null!);

        CreateVehicle(kind).Error.ShouldBe(Vehicle.KindRequiredMessage);
    }

    [Fact]
    public void PreserveKind_On_ReplaceKind_WhenKindIsDefault()
    {
        var vehicle = CreateTraditionalVehicle();
        var original = vehicle.Kind.Value;

        var result = vehicle.ReplaceKind(default);

        result.Error.ShouldBe(Vehicle.KindRequiredMessage);
        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreserveKind_On_ReplaceKind_WhenCaseIsNull(bool traditional)
    {
        var vehicle = CreateTraditionalVehicle();
        var original = vehicle.Kind.Value;
        VehicleKind kind = traditional
            ? new VehicleKind((TraditionalVehicleKind)null!)
            : new VehicleKind((NonTraditionalVehicleKind)null!);

        vehicle.ReplaceKind(kind).Error.ShouldBe(Vehicle.KindRequiredMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Fact]
    public void PreserveRequiredVin_On_RemoveVin_WhenKindIsTraditional()
    {
        var vehicle = CreateTraditionalVehicle();
        var original = vehicle.Kind.Value;

        vehicle.RemoveVin().Error.ShouldBe(Vehicle.InvalidVinMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
        vehicle.VIN.Value.ShouldBe("1HGCM82633A004352");
    }

    [Fact]
    public void ClearVin_On_RemoveVin_WhenKindIsNonTraditional()
    {
        var vehicle = CreateVehicle(NonTraditionalVehicleKind.Create(
            "1HGCM82633A004352", "Trailer", Maybe<string>.None).Value).Value;

        vehicle.RemoveVin().IsSuccess.ShouldBeTrue();

        vehicle.VIN.HasNoValue.ShouldBeTrue();
        vehicle.Make.Value.ShouldBe("Trailer");
        (vehicle.Kind is NonTraditionalVehicleKind).ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NormalizeVin_On_UpdateVin_ForEitherKind(bool traditional)
    {
        var vehicle = CreateForKind(traditional);

        vehicle.UpdateVin(" 1HGCM82633A004352 ").IsSuccess.ShouldBeTrue();

        vehicle.VIN.Value.ShouldBe("1HGCM82633A004352");
        (vehicle.Kind is TraditionalVehicleKind).ShouldBe(traditional);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    [InlineData(true, "invalid")]
    [InlineData(false, null)]
    [InlineData(false, "")]
    [InlineData(false, "   ")]
    [InlineData(false, "invalid")]
    public void PreserveKind_On_UpdateVin_WhenValueIsInvalid(bool traditional, string? vin)
    {
        var vehicle = CreateForKind(traditional);
        var original = vehicle.Kind.Value;

        vehicle.UpdateVin(vin!).Error.ShouldBe(Vehicle.InvalidVinMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NormalizeMake_On_UpdateMake_ForEitherKind(bool traditional)
    {
        var vehicle = CreateForKind(traditional);
        var originalModel = vehicle.Model;

        vehicle.UpdateMake(" Updated ").Value.ShouldBe("Updated");

        vehicle.Make.Value.ShouldBe("Updated");
        vehicle.Model.ShouldBe(originalModel);
        (vehicle.Kind is TraditionalVehicleKind).ShouldBe(traditional);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NormalizeModel_On_UpdateModel_ForEitherKind(bool traditional)
    {
        var vehicle = CreateForKind(traditional);
        var originalMake = vehicle.Make;

        vehicle.UpdateModel(" Updated ").Value.ShouldBe("Updated");

        vehicle.Model.Value.ShouldBe("Updated");
        vehicle.Make.ShouldBe(originalMake);
        (vehicle.Kind is TraditionalVehicleKind).ShouldBe(traditional);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(true, 51)]
    [InlineData(false, 1)]
    [InlineData(false, 51)]
    public void PreserveKind_On_UpdateMake_WhenLengthIsInvalid(bool traditional, int length)
    {
        var vehicle = CreateForKind(traditional);
        var original = vehicle.Kind.Value;

        vehicle.UpdateMake(new string('a', length)).Error.ShouldBe(Vehicle.InvalidLengthMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(true, 51)]
    [InlineData(false, 1)]
    [InlineData(false, 51)]
    public void PreserveKind_On_UpdateModel_WhenLengthIsInvalid(bool traditional, int length)
    {
        var vehicle = CreateForKind(traditional);
        var original = vehicle.Kind.Value;

        vehicle.UpdateModel(new string('a', length)).Error.ShouldBe(Vehicle.InvalidLengthMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    [InlineData(false, null)]
    [InlineData(false, "")]
    [InlineData(false, "   ")]
    public void PreserveKind_On_UpdateMake_WhenValueIsMissing(bool traditional, string? make)
    {
        var vehicle = CreateForKind(traditional);
        var original = vehicle.Kind.Value;

        vehicle.UpdateMake(make!).IsFailure.ShouldBeTrue();

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    [InlineData(false, null)]
    [InlineData(false, "")]
    [InlineData(false, "   ")]
    public void PreserveKind_On_UpdateModel_WhenValueIsMissing(bool traditional, string? model)
    {
        var vehicle = CreateForKind(traditional);
        var original = vehicle.Kind.Value;

        vehicle.UpdateModel(model!).IsFailure.ShouldBeTrue();

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Fact]
    public void PreserveRequiredDescriptions_On_RemoveMakeAndRemoveModel_WhenKindIsTraditional()
    {
        var vehicle = CreateTraditionalVehicle();
        var original = vehicle.Kind.Value;

        vehicle.RemoveMake().Error.ShouldBe(Vehicle.InvalidLengthMessage);
        vehicle.RemoveModel().Error.ShouldBe(Vehicle.InvalidLengthMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Fact]
    public void PreserveLastDescription_On_RemoveMake_WhenModelIsAbsent()
    {
        var vehicle = CreateForKind(false);
        var original = vehicle.Kind.Value;

        vehicle.RemoveMake().Error.ShouldBe(Vehicle.NonTraditionalVehicleInvalidMakeModelMessage);

        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Fact]
    public void ClearMake_On_RemoveMake_WhenModelIsPresent()
    {
        var vehicle = CreateVehicle(NonTraditionalVehicleKind.Create(
            Maybe<string>.None, "Trailer", "Utility").Value).Value;

        vehicle.RemoveMake().IsSuccess.ShouldBeTrue();

        vehicle.Make.HasNoValue.ShouldBeTrue();
        vehicle.Model.Value.ShouldBe("Utility");
        var original = vehicle.Kind.Value;
        vehicle.RemoveModel().Error.ShouldBe(Vehicle.NonTraditionalVehicleInvalidMakeModelMessage);
        vehicle.Kind.Value.ShouldBeSameAs(original);
    }

    [Fact]
    public void ClearModel_On_RemoveModel_WhenMakeIsPresent()
    {
        var vehicle = CreateVehicle(NonTraditionalVehicleKind.Create(
            Maybe<string>.None, "Trailer", "Utility").Value).Value;

        vehicle.RemoveModel().IsSuccess.ShouldBeTrue();

        vehicle.Model.HasNoValue.ShouldBeTrue();
        vehicle.Make.Value.ShouldBe("Trailer");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PreserveAbsence_On_UpdatePlate_WhenValueIsMissing(string? value)
    {
        var vehicle = CreateTraditionalVehicle();

        vehicle.UpdatePlate(value!).Error.ShouldBe(Vehicle.OptionalTextRequiredMessage);

        vehicle.Plate.HasNoValue.ShouldBeTrue();
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenPresentColorIsMissing()
    {
        var result = Vehicle.Create(
            TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord").Value,
            Maybe<int>.None, Maybe<string>.None, Maybe<State>.None,
            Maybe<string>.None, "   ", Today);

        result.Error.ShouldContain(Vehicle.OptionalTextRequiredMessage);
    }

    [Theory]
    [InlineData(true, 1895)]
    [InlineData(true, 2027)]
    [InlineData(false, 1895)]
    [InlineData(false, 2027)]
    public void ReturnFailure_On_Create_WhenYearIsInvalid(bool traditional, int year)
    {
        var result = CreateVehicle(CreateForKind(traditional).Kind, Today, year);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Vehicle.InvalidYearMessage(Today));
    }

    [Theory]
    [InlineData(true, 1896)]
    [InlineData(true, 2026)]
    [InlineData(false, 1896)]
    [InlineData(false, 2026)]
    public void AcceptYear_On_Create_WhenYearIsOnBoundary(bool traditional, int year)
    {
        var result = CreateVehicle(CreateForKind(traditional).Kind, Today, year);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Year.Value.ShouldBe(year);
    }

    [Theory]
    [InlineData(true, 1896)]
    [InlineData(true, 2026)]
    [InlineData(false, 1896)]
    [InlineData(false, 2026)]
    public void ReplaceYear_On_UpdateYear_WhenYearIsOnBoundary(bool traditional, int year)
    {
        var vehicle = CreateForKind(traditional);

        var result = vehicle.UpdateYear(year, Today);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(year);
        vehicle.Year.Value.ShouldBe(year);
    }

    [Theory]
    [InlineData(true, 1895)]
    [InlineData(true, 2027)]
    [InlineData(false, 1895)]
    [InlineData(false, 2027)]
    public void PreserveYear_On_UpdateYear_WhenYearIsInvalid(bool traditional, int year)
    {
        var vehicle = CreateForKind(traditional);
        vehicle.UpdateYear(2020, Today).IsSuccess.ShouldBeTrue();

        var result = vehicle.UpdateYear(year, Today);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Year must be between 1896 and 2026.");
        vehicle.Year.Value.ShouldBe(2020);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UseEvaluationYear_On_Create_WhenDatesCrossNewYear(bool traditional)
    {
        var kind = CreateForKind(traditional).Kind;
        var beforeNewYear = new DateOnly(2025, 12, 31);
        var afterNewYear = new DateOnly(2026, 1, 1);

        CreateVehicle(kind, beforeNewYear, 2027).Error
            .ShouldBe("Year must be between 1896 and 2026.");
        CreateVehicle(kind, afterNewYear, 2027).Value.Year.Value.ShouldBe(2027);
        CreateVehicle(kind, afterNewYear, 2028).Error
            .ShouldBe("Year must be between 1896 and 2027.");
        CreateVehicle(kind, beforeNewYear, 2027).Error
            .ShouldBe("Year must be between 1896 and 2026.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UseEvaluationYear_On_UpdateYear_WhenDatesCrossNewYear(bool traditional)
    {
        var vehicle = CreateForKind(traditional);
        var beforeNewYear = new DateOnly(2025, 12, 31);
        var afterNewYear = new DateOnly(2026, 1, 1);

        vehicle.UpdateYear(2027, beforeNewYear).Error
            .ShouldBe("Year must be between 1896 and 2026.");
        vehicle.Year.HasNoValue.ShouldBeTrue();
        vehicle.UpdateYear(2027, afterNewYear).IsSuccess.ShouldBeTrue();
        vehicle.UpdateYear(2028, afterNewYear).Error
            .ShouldBe("Year must be between 1896 and 2027.");
        vehicle.UpdateYear(2027, beforeNewYear).Error
            .ShouldBe("Year must be between 1896 and 2026.");
        vehicle.Year.Value.ShouldBe(2027);
    }

    [Fact]
    public void ReturnFailure_On_Create_WhenPlateStateProvinceIsUndefined()
    {
        var result = Vehicle.Create(
            TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord").Value,
            Maybe<int>.None, Maybe<string>.None, (State)999,
            Maybe<string>.None, Maybe<string>.None, Today);

        result.Error.ShouldBe(Vehicle.InvalidPlateStateProvinceMessage);
    }

    [Theory]
    [InlineData(true, "Honda Accord")]
    [InlineData(false, "Trailer")]
    public void OmitAbsentValues_On_ToString(bool traditional, string expected) =>
        CreateForKind(traditional).ToString().ShouldBe(expected);

    private static Vehicle CreateForKind(bool traditional) => traditional
        ? CreateTraditionalVehicle()
        : CreateVehicle(NonTraditionalVehicleKind.Create(
            Maybe<string>.None, "Trailer", Maybe<string>.None).Value).Value;

    private static Vehicle CreateTraditionalVehicle() =>
        CreateVehicle(TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", "Accord").Value).Value;

    private static Result<Vehicle> CreateVehicle(VehicleKind kind) =>
        CreateVehicle(kind, Today);

    private static Result<Vehicle> CreateVehicle(VehicleKind kind, DateOnly today, Maybe<int> year = default) =>
        Vehicle.Create(kind, year, Maybe<string>.None, Maybe<State>.None,
            Maybe<string>.None, Maybe<string>.None, today);
}
