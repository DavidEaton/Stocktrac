using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features;

// Aggregate factories reject default or null-case values before using this union.
public union VehicleKind(TraditionalVehicleKind, NonTraditionalVehicleKind)
{
    public Maybe<string> VIN => this switch
    {
        TraditionalVehicleKind traditional => traditional.VIN,
        NonTraditionalVehicleKind nonTraditional => nonTraditional.VIN,
        null => Maybe<string>.None
    };

    public Maybe<string> Make => this switch
    {
        TraditionalVehicleKind traditional => traditional.Make,
        NonTraditionalVehicleKind nonTraditional => nonTraditional.Make,
        null => Maybe<string>.None
    };

    public Maybe<string> Model => this switch
    {
        TraditionalVehicleKind traditional => traditional.Model,
        NonTraditionalVehicleKind nonTraditional => nonTraditional.Model,
        null => Maybe<string>.None
    };
}

public sealed class TraditionalVehicleKind
{
    public string VIN { get; }
    public string Make { get; }
    public string Model { get; }

    private TraditionalVehicleKind(string vin, string make, string model) =>
        (VIN, Make, Model) = (vin, make, model);

    public static Result<TraditionalVehicleKind> Create(string vin, string make, string model)
    {
        var normalizedVin = vin?.Trim() ?? string.Empty;
        var normalizedMake = make?.Trim() ?? string.Empty;
        var normalizedModel = model?.Trim() ?? string.Empty;

        return Result.Success()
            .Ensure(() => normalizedVin.Length == Vehicle.VinRequiredLength, Vehicle.InvalidVinMessage)
            .Bind(() => ValidateMakeModel(normalizedMake))
            .Bind(() => ValidateMakeModel(normalizedModel))
            .Map(() => new TraditionalVehicleKind(normalizedVin, normalizedMake, normalizedModel));
    }

    private static Result ValidateMakeModel(string value) =>
        value.Length < Vehicle.MinimumMakeModelLength || value.Length > Vehicle.MaximumMakeModelLength
            ? Result.Failure(Vehicle.InvalidLengthMessage)
            : Result.Success();
}

public sealed class NonTraditionalVehicleKind
{
    public Maybe<string> VIN { get; }
    public Maybe<string> Make { get; }
    public Maybe<string> Model { get; }

    private NonTraditionalVehicleKind(Maybe<string> vin, Maybe<string> make, Maybe<string> model) =>
        (VIN, Make, Model) = (vin, make, model);

    public static Result<NonTraditionalVehicleKind> Create(
        Maybe<string> vin,
        Maybe<string> make,
        Maybe<string> model)
    {
        var normalizedVin = vin.Map(value => value.Trim());
        var normalizedMake = make.Map(value => value.Trim());
        var normalizedModel = model.Map(value => value.Trim());

        return Result.Success()
            .Ensure(() => normalizedVin.HasNoValue || normalizedVin.Value.Length == Vehicle.VinRequiredLength,
                Vehicle.InvalidVinMessage)
            .Ensure(() => normalizedMake.HasValue || normalizedModel.HasValue,
                Vehicle.NonTraditionalVehicleInvalidMakeModelMessage)
            .Bind(() => ValidateDescription(normalizedMake))
            .Bind(() => ValidateDescription(normalizedModel))
            .Map(() => new NonTraditionalVehicleKind(normalizedVin, normalizedMake, normalizedModel));
    }

    private static Result ValidateDescription(Maybe<string> value) =>
        value.HasValue && string.IsNullOrWhiteSpace(value.Value)
            ? Result.Failure(Vehicle.NonTraditionalVehicleInvalidMakeModelMessage)
            : Result.Success();
}
