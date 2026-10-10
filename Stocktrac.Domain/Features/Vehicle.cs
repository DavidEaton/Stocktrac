using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features;

public sealed class Vehicle : Entity
{
    // TODO: Move these constants to user-configurable settings in the future.
    // For now, they are hard-coded to match the current validation rules in StockTrac.
    public const int MaximumMakeModelLength = 50;
    public const int MinimumMakeModelLength = 2;
    public const int VinRequiredLength = 17;
    public const int MaximumPlateLength = 20;
    public const int MaximumUnitNumberLength = 20;
    public const int MaximumColorLength = 12;

    public const string InvalidVinMessage = "VIN was invalid.";
    public const int YearMinimum = 1896; // First year of production commercial vehicles
    public static int YearMaximum(DateOnly today) => today.Year + 1;
    public static string InvalidYearMessage(DateOnly today) =>
        $"Year must be between {YearMinimum} and {YearMaximum(today)}.";
    public static readonly string InvalidLengthMessage = $"Make, Model must be between {MinimumMakeModelLength} and {MaximumMakeModelLength} characters in length.";
    public const string NonTraditionalVehicleInvalidMakeModelMessage = "Please enter Make or Model.";
    public static string InvalidMaximumLengthMessage(int max) => $"Value must be less than {max} characters in length.";
    public const string InvalidPlateStateProvinceMessage = "Plate State/Province is invalid.";
    public const string OptionalTextRequiredMessage = "Use the remove operation to clear an optional value.";

    public const string KindRequiredMessage = "Vehicle kind is required.";

    public VehicleKind Kind { get; private set; }
    public Maybe<string> VIN => Kind.VIN;
    public Maybe<int> Year { get; private set; }
    public Maybe<string> Make => Kind.Make;
    public Maybe<string> Model => Kind.Model;
    public Maybe<string> Plate { get; private set; }
    public Maybe<State> PlateStateProvince { get; private set; }
    public Maybe<string> UnitNumber { get; private set; }
    public Maybe<string> Color { get; private set; }
    public bool Active { get; private set; } = true;

    public override string ToString() => string.Join(" ",
        new[] { Year.Map(value => value.ToString()), Make, Model }
            .Where(value => value.HasValue)
            .Select(value => value.Value));

    private Vehicle(
        VehicleKind kind,
        Maybe<int> year,
        Maybe<string> plate,
        Maybe<State> plateStateProvince,
        Maybe<string> unitNumber,
        Maybe<string> color,
        bool active)
    {
        Kind = kind;
        Year = year;
        Plate = plate;
        PlateStateProvince = plateStateProvince;
        UnitNumber = unitNumber;
        Color = color;
        Active = active;
    }

    public static Result<Vehicle> Create(
        VehicleKind kind,
        Maybe<int> year,
        Maybe<string> plate,
        Maybe<State> plateStateProvince,
        Maybe<string> unitNumber,
        Maybe<string> color,
        DateOnly today,
        bool active = true)
    {
        var normalizedPlate = plate.Map(value => value.Trim());
        var normalizedUnitNumber = unitNumber.Map(value => value.Trim());
        var normalizedColor = color.Map(value => value.Trim());
        return Result.Combine(
                Environment.NewLine,
                Result.FailureIf(kind.Value is null, KindRequiredMessage),
                ValidateYear(year, today),
                ValidatePlate(normalizedPlate),
                ValidatePlateStateProvince(plateStateProvince),
                ValidateUnitNumber(normalizedUnitNumber),
                ValidateColor(normalizedColor))
            .Map(() => new Vehicle(
                kind,
                year,
                normalizedPlate,
                plateStateProvince,
                normalizedUnitNumber,
                normalizedColor,
                active));
    }

    private static Result ValidateYear(Maybe<int> year, DateOnly today) =>
        year.HasValue && (year.Value > YearMaximum(today) || year.Value < YearMinimum)
            ? Result.Failure(InvalidYearMessage(today))
            : Result.Success();

    private static Result ValidatePlate(Maybe<string> plate)
    {
        return ValidateOptionalText(plate, MaximumPlateLength);
    }

    private static Result ValidatePlateStateProvince(Maybe<State> plateStateProvince) =>
        plateStateProvince.HasNoValue || Enum.IsDefined(plateStateProvince.Value)
            ? Result.Success()
            : Result.Failure(InvalidPlateStateProvinceMessage);

    private static Result ValidateUnitNumber(Maybe<string> unitNumber)
    {
        return ValidateOptionalText(unitNumber, MaximumUnitNumberLength);
    }

    private static Result ValidateColor(Maybe<string> color)
    {
        return ValidateOptionalText(color, MaximumColorLength);
    }

    private static Result ValidateOptionalText(Maybe<string> value, int maximumLength)
    {
        if (value.HasNoValue)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(value.Value))
            return Result.Failure(OptionalTextRequiredMessage);

        return value.Value.Length <= maximumLength
            ? Result.Success()
            : Result.Failure(InvalidMaximumLengthMessage(maximumLength));
    }

    public Result ReplaceKind(VehicleKind kind) =>
        kind.Value is null
            ? Result.Failure(KindRequiredMessage)
            : Result.Success().Tap(() => Kind = kind);

    public Result<Maybe<string>> UpdateVin(string vin)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return Result.Failure<Maybe<string>>(InvalidVinMessage);

        var updatedKind = Kind switch
        {
            TraditionalVehicleKind traditional => TraditionalVehicleKind.Create(vin, traditional.Make, traditional.Model)
                .Map(value => (VehicleKind)value),
            NonTraditionalVehicleKind nonTraditional => NonTraditionalVehicleKind.Create(vin, nonTraditional.Make, nonTraditional.Model)
                .Map(value => (VehicleKind)value),
            null => Result.Failure<VehicleKind>(KindRequiredMessage)
        };
        return updatedKind.Tap(value => Kind = value).Map(value => value.VIN);
    }

    public Result RemoveVin() => Kind switch
    {
        TraditionalVehicleKind => Result.Failure(InvalidVinMessage),
        NonTraditionalVehicleKind nonTraditional => NonTraditionalVehicleKind.Create(
                Maybe<string>.None, nonTraditional.Make, nonTraditional.Model)
            .Tap(value => Kind = value),
        null => Result.Failure(KindRequiredMessage)
    };

    public Result<Maybe<int>> UpdateYear(int year, DateOnly today) =>
        ValidateYear(year, today)
            .Map(() => Year = year);

    public void RemoveYear() => Year = Maybe<int>.None;

    public Result<string> UpdateMake(string make)
    {
        make = make?.Trim() ?? string.Empty;
        if (make.Length < MinimumMakeModelLength || make.Length > MaximumMakeModelLength)
            return Result.Failure<string>(InvalidLengthMessage);

        var updatedKind = Kind switch
        {
            TraditionalVehicleKind traditional => TraditionalVehicleKind.Create(traditional.VIN, make, traditional.Model)
                .Map(value => (VehicleKind)value),
            NonTraditionalVehicleKind nonTraditional => NonTraditionalVehicleKind.Create(nonTraditional.VIN, make, nonTraditional.Model)
                .Map(value => (VehicleKind)value),
            null => Result.Failure<VehicleKind>(KindRequiredMessage)
        };
        return updatedKind.Tap(value => Kind = value).Map(value => value.Make.Value);
    }

    public Result<string> UpdateModel(string model)
    {
        model = model?.Trim() ?? string.Empty;
        if (model.Length < MinimumMakeModelLength || model.Length > MaximumMakeModelLength)
            return Result.Failure<string>(InvalidLengthMessage);

        var updatedKind = Kind switch
        {
            TraditionalVehicleKind traditional => TraditionalVehicleKind.Create(traditional.VIN, traditional.Make, model)
                .Map(value => (VehicleKind)value),
            NonTraditionalVehicleKind nonTraditional => NonTraditionalVehicleKind.Create(nonTraditional.VIN, nonTraditional.Make, model)
                .Map(value => (VehicleKind)value),
            null => Result.Failure<VehicleKind>(KindRequiredMessage)
        };
        return updatedKind.Tap(value => Kind = value).Map(value => value.Model.Value);
    }

    public Result RemoveMake() => Kind switch
    {
        TraditionalVehicleKind => Result.Failure(InvalidLengthMessage),
        NonTraditionalVehicleKind nonTraditional => NonTraditionalVehicleKind.Create(
                nonTraditional.VIN, Maybe<string>.None, nonTraditional.Model)
            .Tap(value => Kind = value),
        null => Result.Failure(KindRequiredMessage)
    };

    public Result RemoveModel() => Kind switch
    {
        TraditionalVehicleKind => Result.Failure(InvalidLengthMessage),
        NonTraditionalVehicleKind nonTraditional => NonTraditionalVehicleKind.Create(
                nonTraditional.VIN, nonTraditional.Make, Maybe<string>.None)
            .Tap(value => Kind = value),
        null => Result.Failure(KindRequiredMessage)
    };

    public Result<Maybe<string>> UpdatePlate(string plate)
    {
        plate = plate?.Trim() ?? string.Empty;
        return ValidateOptionalText(plate, MaximumPlateLength)
            .Map(() => Plate = plate);
    }

    public void RemovePlate() => Plate = Maybe<string>.None;

    public Result<Maybe<State>> UpdatePlateStateProvince(State plateStateProvince) =>
        !Enum.IsDefined(plateStateProvince)
            ? Result.Failure<Maybe<State>>(InvalidPlateStateProvinceMessage)
            : Result.Success(PlateStateProvince = plateStateProvince);

    public void RemovePlateStateProvince() => PlateStateProvince = Maybe<State>.None;

    public Result<Maybe<string>> UpdateUnitNumber(string unitNumber)
    {
        unitNumber = unitNumber?.Trim() ?? string.Empty;
        return ValidateOptionalText(unitNumber, MaximumUnitNumberLength)
            .Map(() => UnitNumber = unitNumber);
    }

    public void RemoveUnitNumber() => UnitNumber = Maybe<string>.None;

    public Result<Maybe<string>> UpdateColor(string color)
    {
        color = color?.Trim() ?? string.Empty;
        return ValidateOptionalText(color, MaximumColorLength)
            .Map(() => Color = color);
    }

    public void RemoveColor() => Color = Maybe<string>.None;

    public void UpdateActive(bool active = true) => Active = active;
}
