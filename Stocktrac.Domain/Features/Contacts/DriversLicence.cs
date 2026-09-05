using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public readonly record struct DriversLicense
{
    public static readonly string StateInvalidMessage = $"Please enter a valid State";
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
        Result.Success(state)
            .Ensure(Enum.IsDefined, StateInvalidMessage)
            .Map(validState => new DriversLicense(number, validState, validRange));

    public Result<DriversLicense> NewNumber(DriversLicenseNumber newNumber) =>
        Create(newNumber, State, ValidDateRange);

    public Result<DriversLicense> NewState(State newState) =>
        !Enum.IsDefined(newState)
            ? Result.Failure<DriversLicense>(StateInvalidMessage)
            : Create(Number, newState, ValidDateRange);

    public Result<DriversLicense> NewValidDateRange(DateTimeRange dateRange) =>
        Create(Number, State, dateRange);
}
