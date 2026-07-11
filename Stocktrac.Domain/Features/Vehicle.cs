using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contact;

namespace Stocktrac.Domain.Features;

public class Vehicle : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public static readonly int MaximumMakeModelLength = 50;
    public static readonly int MinimumMakeModelLength = 2;
    public static readonly int VinRequiredLength = 17;
    public static readonly int MaximumPlateLength = 20;
    public static readonly int MaximumUnitNumberLength = 20;
    public static readonly int MaximumColorLength = 12;

    public static readonly string InvalidVinMessage = $"VIN was invalid";
    public const int YearMinimum = 1896; // First year of production commercial vehicles
    public static readonly string InvalidYearMessage = $"Year must be between {YearMinimum} and {DateTime.Today.Year + 1}";
    public static readonly string InvalidLengthMessage = $"Make, Model must be between {MinimumMakeModelLength} and {MaximumMakeModelLength} characters in length";
    public static readonly string NonTraditionalVehicleInvalidMakeModelMessage = $"Please enter Make or Model";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length";
    public static readonly string InvalidPlateStateProvinceMessage = $"Plate State/Province is invalid";

    public string VIN { get; private set; } // Refactor to ValueObject
    public int? Year { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public bool NonTraditionalVehicle { get; private set; } = false; // We need to allow for non-traditional vehicles. For example, they may be servicing a trailer and just type in TRAILER for the Make and nothing else.
    public string Plate { get; private set; }
    public State? PlateStateProvince { get; private set; }
    public string UnitNumber { get; private set; }
    public string Color { get; private set; }
    public bool Active { get; private set; } = true;

    public override string ToString() => $"{Year ?? 0} {Make} {Model}";

    private Vehicle(
        string vin,
        int? year,
        string make,
        string model,
        bool nonTraditionalVehicle,
        string plate,
        State? plateStateProvince,
        string unitNumber,
        string color,
        bool active)
    {
        VIN = vin;
        Year = year;
        Make = make;
        Model = model;
        NonTraditionalVehicle = nonTraditionalVehicle;
        Plate = plate;
        PlateStateProvince = plateStateProvince;
        UnitNumber = unitNumber;
        Color = color;
        Active = active;
    }

    public static Result<Vehicle> Create(
        string vin,
        int? year,
        string make,
        string model,
        string plate,
        State? plateStateProvince,
        string unitNumber,
        string color,
        bool active = true,
        bool nonTraditionalVehicle = false)
    {
        make = (make ?? string.Empty).Trim();
        model = (model ?? string.Empty).Trim();
        plate = (plate ?? string.Empty).Trim();
        unitNumber = (unitNumber ?? string.Empty).Trim();
        color = (color ?? string.Empty).Trim();

        var vinResult = ValidateVin(vin, nonTraditionalVehicle);
        if (vinResult.IsFailure)
            return Result.Failure<Vehicle>(vinResult.Error);

        var makeModelResult = ValidateMakeModel(make, model, nonTraditionalVehicle);
        if (makeModelResult.IsFailure)
            return Result.Failure<Vehicle>(makeModelResult.Error);

        var yearResult = ValidateYear(year);
        if (yearResult.IsFailure)
            return Result.Failure<Vehicle>(yearResult.Error);

        var plateResult = ValidatePlate(plate);
        if (plateResult.IsFailure)
            return Result.Failure<Vehicle>(plateResult.Error);

        var plateStateProvinceResult = ValidatePlateStateProvince(plateStateProvince);
        if (plateStateProvinceResult.IsFailure)
            return Result.Failure<Vehicle>(plateStateProvinceResult.Error);

        var unitNumberResult = ValidateUnitNumber(unitNumber);
        if (unitNumberResult.IsFailure)
            return Result.Failure<Vehicle>(unitNumberResult.Error);

        var colorResult = ValidateColor(color);
        if (colorResult.IsFailure)
            return Result.Failure<Vehicle>(colorResult.Error);

        return Result.Success(
            new Vehicle(
                vin: vin,
                year: year,
                make: make,
                model: model,
                nonTraditionalVehicle: nonTraditionalVehicle,
                plate: plate,
                plateStateProvince: plateStateProvince,
                unitNumber: unitNumber,
                color: color,
                active: active));
    }

    private static Result ValidateMakeModel(string make, string model, bool nonTraditionalVehicle)
    {
        if (!nonTraditionalVehicle)
        {
            if (string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model))
                return Result.Failure(InvalidLengthMessage);

            if (make.Length < MinimumMakeModelLength || make.Length > MaximumMakeModelLength ||
                model.Length < MinimumMakeModelLength || model.Length > MaximumMakeModelLength)
                return Result.Failure(InvalidLengthMessage);
        }

        if (nonTraditionalVehicle)
            if (string.IsNullOrWhiteSpace(make) && string.IsNullOrWhiteSpace(model))
                return Result.Failure(NonTraditionalVehicleInvalidMakeModelMessage);

        return Result.Success();
    }

    private static Result ValidateVin(
        string? vin,
        bool nonTraditionalVehicle)
    {
        if (vin is null)
        {
            return nonTraditionalVehicle
                ? Result.Success()
                : Result.Failure(InvalidVinMessage);
        }

        if (vin.Length != VinRequiredLength)
            return Result.Failure(InvalidVinMessage);

        return Result.Success();
    }

    private static Result ValidateYear(int? year) =>
        year > DateTime.Today.Year + 1 || year < YearMinimum
            ? Result.Failure(InvalidYearMessage)
            : Result.Success();

    private static Result ValidatePlate(string? plate)
    {
        plate = plate?.Trim() ?? string.Empty;
        return plate.Length > MaximumPlateLength
            ? Result.Failure(
                InvalidMaximumLengthMessage(MaximumPlateLength))
            : Result.Success();
    }

    private static Result ValidatePlateStateProvince(State? plateStateProvince) =>
        plateStateProvince is null || Enum.IsDefined(plateStateProvince.Value)
            ? Result.Success()
            : Result.Failure(InvalidPlateStateProvinceMessage);

    private static Result ValidateUnitNumber(string? unitNumber)
    {
        unitNumber = unitNumber?.Trim() ?? string.Empty;
        return unitNumber.Length > MaximumUnitNumberLength
            ? Result.Failure(
                InvalidMaximumLengthMessage(MaximumUnitNumberLength))
            : Result.Success();
    }

    private static Result ValidateColor(string? color)
    {
        color = color?.Trim() ?? string.Empty;
        return color.Length > MaximumColorLength
            ? Result.Failure(
                InvalidMaximumLengthMessage(MaximumColorLength))
            : Result.Success();
    }

    public Result<string> SetVin(string vin)
    {
        vin = (vin ?? string.Empty).Trim();
        return vin.Length.Equals(VinRequiredLength)
            ? Result.Success(VIN = vin)
            : Result.Failure<string>(InvalidVinMessage);
    }

    public Result<int?> SetYear(int? year) =>
        year > DateTime.Today.Year + 1 || year < YearMinimum
            ? Result.Failure<int?>(InvalidYearMessage)
            : Result.Success(Year = year);

    public Result<string> SetMake(string make)
    {
        make = (make ?? string.Empty).Trim();
        return make.Length < MinimumMakeModelLength || make.Length > MaximumMakeModelLength
            ? Result.Failure<string>(InvalidLengthMessage)
            : Result.Success(Make = make);
    }

    public Result<string> SetModel(string model)
    {
        model = (model ?? string.Empty).Trim();
        return model.Length < MinimumMakeModelLength || model.Length > MaximumMakeModelLength
            ? Result.Failure<string>(InvalidLengthMessage)
            : Result.Success(Model = model);
    }

    public Result<string> SetPlate(string? plate)
    {
        plate = plate?.Trim() ?? string.Empty;
        return plate.Length > MaximumPlateLength
            ? Result.Failure<string>(
                InvalidMaximumLengthMessage(MaximumPlateLength))
            : Result.Success(Plate = plate);
    }

    public Result<State?> SetPlateStateProvince(State? plateStateProvince) =>
        plateStateProvince is not null && !Enum.IsDefined(plateStateProvince.Value)
            ? Result.Failure<State?>(InvalidPlateStateProvinceMessage)
            : Result.Success(PlateStateProvince = plateStateProvince);

    public Result<string> SetUnitNumber(string? unitNumber)
    {
        unitNumber = unitNumber?.Trim() ?? string.Empty;
        return unitNumber.Length > MaximumUnitNumberLength
            ? Result.Failure<string>(
                InvalidMaximumLengthMessage(MaximumUnitNumberLength))
            : Result.Success(UnitNumber = unitNumber);
    }

    public Result<string> SetColor(string? color)
    {
        color = color?.Trim() ?? string.Empty;
        return color.Length > MaximumColorLength
            ? Result.Failure<string>(
                InvalidMaximumLengthMessage(MaximumColorLength))
            : Result.Success(Color = color);
    }

    public Result<bool> SetActive(bool active = true) =>
        Result.Success(Active = active);

    public Result<bool> SetNonTraditionalVehicle(bool nonTraditionalVehicle) =>
        // TODO: MUST update properties to be valid if the vehicle is now a non-traditional vehicle
        Result.Success(NonTraditionalVehicle = nonTraditionalVehicle);

    // EF requires a parameterless constructor
    private Vehicle()
    {
        Make = string.Empty;
        Model = string.Empty;
        Plate = string.Empty;
        UnitNumber = string.Empty;
        Color = string.Empty;
        VIN = string.Empty;
    }
}
