using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DriversLicense
{
    public const string RequiredMessage = "Driver's license details are required.";
    public const string StateInvalidMessage = "Please enter a valid State.";
    public DriversLicenseNumber Number { get; }
    public DateTimeRange ValidDateRange { get; }
    public State State { get; }

    private DriversLicense(
        DriversLicenseNumber number,
        State state,
        DateTimeRange validDateRange)
    {
        Number = number;
        State = state;
        ValidDateRange = validDateRange;
    }

    public static Result<DriversLicense> Create(DriversLicenseNumber number, State state, DateTimeRange validRange) =>
        Result.Combine(
                Environment.NewLine,
                Result.FailureIf(number is null, RequiredMessage),
                Result.FailureIf(!Enum.IsDefined(state), StateInvalidMessage))
            .Map(() => new DriversLicense(number!, state, validRange));

    public Result<DriversLicense> WithNumber(DriversLicenseNumber newNumber) =>
        Create(newNumber, State, ValidDateRange);

    public Result<DriversLicense> WithState(State newState) =>
        Create(Number, newState, ValidDateRange);

    public Result<DriversLicense> WithValidDateRange(DateTimeRange dateRange) =>
        Create(Number, State, dateRange);
}
