using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features;

public class Vehicle : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MaximumMakeModelLength = 50;
    public const int MinimumMakeModelLength = 2;
    public const int VinRequiredLength = 17;
    public const int MaximumPlateLength = 20;
    public const int MaximumUnitNumberLength = 20;
    public const int MaximumColorLength = 12;

    public static readonly string InvalidVinMessage = $"VIN was invalid.";
    public const int YearMinimum = 1896; // First year of production commercial vehicles
    public static readonly string InvalidYearMessage = $"Year must be between {YearMinimum} and {DateTime.Today.Year + 1}.";
    public static readonly string InvalidLengthMessage = $"Make, Model must be between {MinimumMakeModelLength} and {MaximumMakeModelLength} characters in length.";
    public static readonly string NonTraditionalVehicleInvalidMakeModelMessage = $"Please enter Make or Model.";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length.";
    public static readonly string InvalidPlateStateProvinceMessage = $"Plate State/Province is invalid.";

    public Maybe<string> VIN { get; private set; } // Refactor to ValueObject
    public Maybe<int> Year { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public bool NonTraditionalVehicle { get; private set; } = false; // We need to allow for non-traditional vehicles. For example, they may be servicing a trailer and just type in TRAILER for the Make and nothing else.
    public Maybe<string> Plate { get; private set; }
    public Maybe<State> PlateStateProvince { get; private set; }
    public Maybe<string> UnitNumber { get; private set; }
    public Maybe<string> Color { get; private set; }
    public bool Active { get; private set; } = true;

    public override string ToString() => $"{Year.GetValueOrDefault()} {Make} {Model}";

    private Vehicle(
        Maybe<string> vin,
        Maybe<int> year,
        string make,
        string model,
        bool nonTraditionalVehicle,
        Maybe<string> plate,
        Maybe<State> plateStateProvince,
        Maybe<string> unitNumber,
        Maybe<string> color,
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
        Maybe<string> vin,
        Maybe<int> year,
        string make,
        string model,
        Maybe<string> plate,
        Maybe<State> plateStateProvince,
        Maybe<string> unitNumber,
        Maybe<string> color,
        bool active = true,
        bool nonTraditionalVehicle = false)
    {
        var normalizedMake = make.Trim();
        var normalizedModel = model.Trim();
        var normalizedVin = vin.Map(value => value.Trim());
        var normalizedPlate = plate.Map(value => value.Trim());
        var normalizedUnitNumber = unitNumber.Map(value => value.Trim());
        var normalizedColor = color.Map(value => value.Trim());
        return Result.Combine(
                Environment.NewLine,
                ValidateVin(normalizedVin, nonTraditionalVehicle),
                ValidateMakeModel(normalizedMake, normalizedModel, nonTraditionalVehicle),
                ValidateYear(year),
                ValidatePlate(normalizedPlate),
                ValidatePlateStateProvince(plateStateProvince),
                ValidateUnitNumber(normalizedUnitNumber),
                ValidateColor(normalizedColor))
            .Map(() => new Vehicle(
                normalizedVin,
                year,
                normalizedMake,
                normalizedModel,
                nonTraditionalVehicle,
                normalizedPlate,
                plateStateProvince,
                normalizedUnitNumber,
                normalizedColor,
                active));
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
        Maybe<string> vin,
        bool nonTraditionalVehicle)
    {
        if (vin.HasNoValue)
        {
            return nonTraditionalVehicle
                ? Result.Success()
                : Result.Failure(InvalidVinMessage);
        }

        if (vin.Value.Length != VinRequiredLength)
            return Result.Failure(InvalidVinMessage);

        return Result.Success();
    }

    private static Result ValidateYear(Maybe<int> year) =>
        year.HasValue && (year.Value > DateTime.Today.Year + 1 || year.Value < YearMinimum)
            ? Result.Failure(InvalidYearMessage)
            : Result.Success();

    private static Result ValidatePlate(Maybe<string> plate)
    {
        return plate.HasValue && plate.Value.Length > MaximumPlateLength
            ? Result.Failure(
                InvalidMaximumLengthMessage(MaximumPlateLength))
            : Result.Success();
    }

    private static Result ValidatePlateStateProvince(Maybe<State> plateStateProvince) =>
        plateStateProvince.HasNoValue || Enum.IsDefined(plateStateProvince.Value)
            ? Result.Success()
            : Result.Failure(InvalidPlateStateProvinceMessage);

    private static Result ValidateUnitNumber(Maybe<string> unitNumber)
    {
        return unitNumber.HasValue && unitNumber.Value.Length > MaximumUnitNumberLength
            ? Result.Failure(
                InvalidMaximumLengthMessage(MaximumUnitNumberLength))
            : Result.Success();
    }

    private static Result ValidateColor(Maybe<string> color)
    {
        return color.HasValue && color.Value.Length > MaximumColorLength
            ? Result.Failure(
                InvalidMaximumLengthMessage(MaximumColorLength))
            : Result.Success();
    }

    public Result<Maybe<string>> SetVin(string vin)
    {
        vin = vin.Trim();
        return vin.Length.Equals(VinRequiredLength)
            ? Result.Success(VIN = vin)
            : Result.Failure<Maybe<string>>(InvalidVinMessage);
    }

    public Result ClearVin() => NonTraditionalVehicle
        ? Result.Success().Tap(() => VIN = Maybe<string>.None)
        : Result.Failure(InvalidVinMessage);

    public Result<Maybe<int>> SetYear(int year) =>
        year > DateTime.Today.Year + 1 || year < YearMinimum
            ? Result.Failure<Maybe<int>>(InvalidYearMessage)
            : Result.Success(Year = year);

    public void ClearYear() => Year = Maybe<int>.None;

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

    public Result<Maybe<string>> SetPlate(string plate)
    {
        plate = plate.Trim();
        return plate.Length > MaximumPlateLength
            ? Result.Failure<Maybe<string>>(
                InvalidMaximumLengthMessage(MaximumPlateLength))
            : Result.Success(Plate = plate);
    }

    public void ClearPlate() => Plate = Maybe<string>.None;

    public Result<Maybe<State>> SetPlateStateProvince(State plateStateProvince) =>
        !Enum.IsDefined(plateStateProvince)
            ? Result.Failure<Maybe<State>>(InvalidPlateStateProvinceMessage)
            : Result.Success(PlateStateProvince = plateStateProvince);

    public void ClearPlateStateProvince() => PlateStateProvince = Maybe<State>.None;

    public Result<Maybe<string>> SetUnitNumber(string unitNumber)
    {
        unitNumber = unitNumber.Trim();
        return unitNumber.Length > MaximumUnitNumberLength
            ? Result.Failure<Maybe<string>>(
                InvalidMaximumLengthMessage(MaximumUnitNumberLength))
            : Result.Success(UnitNumber = unitNumber);
    }

    public void ClearUnitNumber() => UnitNumber = Maybe<string>.None;

    public Result<Maybe<string>> SetColor(string color)
    {
        color = color.Trim();
        return color.Length > MaximumColorLength
            ? Result.Failure<Maybe<string>>(
                InvalidMaximumLengthMessage(MaximumColorLength))
            : Result.Success(Color = color);
    }

    public void ClearColor() => Color = Maybe<string>.None;

    public void SetActive(bool active = true) => Active = active;

    public Result SetNonTraditionalVehicle(bool nonTraditionalVehicle) =>
        Result.Combine(
                ValidateVin(VIN, nonTraditionalVehicle),
                ValidateMakeModel(Make, Model, nonTraditionalVehicle))
            .Tap(() => NonTraditionalVehicle = nonTraditionalVehicle);

    // EF requires a parameterless constructor
    private Vehicle()
    {
        Make = string.Empty;
        Model = string.Empty;
        Plate = Maybe<string>.None;
        UnitNumber = Maybe<string>.None;
        Color = Maybe<string>.None;
        VIN = Maybe<string>.None;
    }
}
