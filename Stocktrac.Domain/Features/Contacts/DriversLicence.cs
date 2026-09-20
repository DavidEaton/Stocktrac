using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

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

    public DriversLicenseNumber Number { get; private init; }

    public DateTimeRange ValidDateRange { get; private init; }

    public State State { get; private init; }

    private DriversLicense(
        DriversLicenseNumber number,
        State state,
        DateTimeRange dateRange)
    {
        Number = number;
        State = state;
        ValidDateRange = dateRange;
    }

    public static Result<DriversLicense> Create(
        DriversLicenseNumber number,
        State state,
        DateTimeRange dateRange,
        DateOnly today)
    {
        var numberResult =
            number.AsValidDriversLicenseNumber();

        var stateResult =
            state.AsValidDriversLicenseState();

        var dateRangeResult =
            dateRange.AsValidDriversLicenseDateTimeRange(today);

        return Result.Combine(
                Environment.NewLine,
                numberResult,
                stateResult,
                dateRangeResult)
            .Map(() =>
                new DriversLicense(
                    numberResult.Value,
                    stateResult.Value,
                    dateRangeResult.Value));
    }

    public Result<DriversLicense> WithNumber(
        DriversLicenseNumber number) =>
        number.AsValidDriversLicenseNumber()
            .Map(validNumber =>
                this with { Number = validNumber });

    public Result<DriversLicense> WithState(State state) =>
        state.AsValidDriversLicenseState()
            .Map(validState =>
                this with { State = validState });

    public Result<DriversLicense> WithValidDateRange(
        DateTimeRange dateRange,
        DateOnly today) =>
        dateRange.AsValidDriversLicenseDateTimeRange(today)
            .Map(validDateRange =>
                this with { ValidDateRange = validDateRange });

    public bool IsExpired(DateOnly today) =>
        ValidDateRange.End < today;
}
