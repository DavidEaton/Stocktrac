using Shouldly;
using Stocktrac.Domain.Features;

namespace Stocktrac.Tests.Features.Unit;

public class TraditionalVehicleKindShould
{
    [Fact]
    public void ReturnFirstError_On_Create_WhenSeveralInputsAreInvalid()
    {
        var result = TraditionalVehicleKind.Create("invalid", "", "");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Vehicle.InvalidVinMessage);
    }

    [Fact]
    public void NormalizeRequiredValues_On_Create_WhenValuesAreValid()
    {
        var result = TraditionalVehicleKind.Create(" 1HGCM82633A004352 ", " Honda ", " Accord ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.VIN.ShouldBe("1HGCM82633A004352");
        result.Value.Make.ShouldBe("Honda");
        result.Value.Model.ShouldBe("Accord");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1HGCM82633A00435")]
    [InlineData("1HGCM82633A0043520")]
    public void ReturnFailure_On_Create_WhenVinIsInvalid(string? vin) =>
        TraditionalVehicleKind.Create(vin!, "Honda", "Accord").Error.ShouldBe(Vehicle.InvalidVinMessage);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    public void EnforceMakeLength_On_Create(int length, bool valid) =>
        TraditionalVehicleKind.Create("1HGCM82633A004352", new string('a', length), "Accord")
            .IsSuccess.ShouldBe(valid);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    public void EnforceModelLength_On_Create(int length, bool valid) =>
        TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", new string('a', length))
            .IsSuccess.ShouldBe(valid);

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void ReturnFailure_On_Create_WhenMakeIsMissing(string? make) =>
        TraditionalVehicleKind.Create("1HGCM82633A004352", make!, "Accord")
            .Error.ShouldBe(Vehicle.InvalidLengthMessage);

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void ReturnFailure_On_Create_WhenModelIsMissing(string? model) =>
        TraditionalVehicleKind.Create("1HGCM82633A004352", "Honda", model!)
            .Error.ShouldBe(Vehicle.InvalidLengthMessage);
}
