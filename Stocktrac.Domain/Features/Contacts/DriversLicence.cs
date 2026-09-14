using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DriversLicense
{
    public const string RequiredMessage = "Driver's license details are required.";
    public const string StateInvalidMessage = "Please enter a valid State.";
    public DriversLicenseNumber Number { get; private set; }
    public DateTimeRange ValidDateRange { get; private set; }
    public State State { get; private set; }

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
                number.AsValidDriversLicenseNumber(validRange),
                state.AsValidDriversLicenseState())
            .Map(() => new DriversLicense(number!, state, validRange!));

    public Result<DriversLicense> WithNumber(DriversLicenseNumber newNumber) =>
        newNumber.AsValidDriversLicenseNumber(ValidDateRange)
            .Map(validNumber => this with { Number = validNumber });

    public Result<DriversLicense> WithState(State newState) =>
        newState.AsValidDriversLicenseState()
            .Map(validState => this with { State = validState });

    public Result<DriversLicense> WithValidDateRange(DateTimeRange dateRange) =>
        Number.AsValidDriversLicenseNumber(dateRange)
            .Map(_ => this with { ValidDateRange = dateRange });
}
