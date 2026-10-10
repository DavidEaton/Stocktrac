using CSharpFunctionalExtensions;
using Stocktrac.Domain.Features.Contacts;

namespace Stocktrac.Domain.Features.Persons;

public sealed record DriversLicense
{
    public static readonly DateOnly MinimumValidDate =
        new(1900, 1, 1);

    public const int MaximumValidityYears = 50;

    public const string RequiredMessage =
        "Driver's license details are required.";

    public const string StateInvalidMessage =
        "Please enter a valid state.";

    public const string DateOrderInvalidMessage =
        "The driver's license start date must be before its end date.";

    public static readonly string StartDateTooEarlyMessage =
        $"The driver's license start date cannot be earlier than {MinimumValidDate:d}.";

    public const string StartDateInFutureMessage =
        "The driver's license start date cannot be in the future.";

    public static readonly string DateRangeTooLongMessage =
        $"The driver's license validity period cannot exceed {MaximumValidityYears} years.";

    public DriversLicenseNumber Number { get; }
    public DateRange ValidDateRange { get; }
    public State State { get; }

    private DriversLicense(
        DriversLicenseNumber number,
        State state,
        DateRange dateRange)
    {
        Number = number;
        State = state;
        ValidDateRange = dateRange;
    }

    public static Result<DriversLicense> Create(
        DriversLicenseNumber number,
        State state,
        DateRange dateRange,
        DateOnly today) =>
        state.AsValidState()
            .Bind(validState => dateRange.AsValidDriversLicenseDateRange(today)
                .Map(validDateRange => new DriversLicense(
                    number,
                    validState,
                    validDateRange)));

    public Result<DriversLicense> ReplaceNumber(DriversLicenseNumber number) =>
        Result.Success(
            new DriversLicense(number, State, ValidDateRange));

    public Result<DriversLicense> ReplaceState(State state) =>
        state.AsValidState()
            .Map(validState =>
                new DriversLicense(Number, validState, ValidDateRange));

    public Result<DriversLicense> ReplaceValidDateRange(DateRange dateRange, DateOnly today) =>
        dateRange.AsValidDriversLicenseDateRange(today)
            .Map(validDateRange =>
                new DriversLicense(Number, State, validDateRange));

    public bool IsActive(DateOnly today) =>
        ValidDateRange.Start <= today && today <= ValidDateRange.End;

    public bool IsExpired(DateOnly today) =>
        ValidDateRange.End < today;

    public bool IsNotYetValid(DateOnly today) =>
        today < ValidDateRange.Start;
}
