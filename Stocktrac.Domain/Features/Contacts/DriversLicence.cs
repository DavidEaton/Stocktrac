using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public class DriversLicense : ValueObject
{
    public static readonly int MinimumLength = 3;
    public static readonly int MaximumLength = 255;
    public static readonly string UnderMinimumLengthMessage = $"Drivers License cannot be less than {MinimumLength} character(s) in length";
    public static readonly string OverMaximumLengthMessage = $"Drivers License cannot be over {MaximumLength} characters in length";
    public static readonly string DateRangeInvalidMessage = $"Drivers License must have valid dates";
    public static readonly string RequiredMessage = $"Drivers License number is required.";
    public static readonly string StateInvalidMessage = $"Please enter a valid State";

    public string? Number { get; private set; }
    public DateTimeRange ValidDateRange { get; private set; }
    public State State { get; private set; }

    private DriversLicense(
        string number,
        State state,
        DateTimeRange validDateRange)
    {
        Number = number;
        State = state;
        ValidDateRange = validDateRange;
    }

    public static Result<DriversLicense> Create(
        string number,
        State state,
        DateTimeRange validRange)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<DriversLicense>(RequiredMessage);

        if (!Enum.IsDefined(state))
            return Result.Failure<DriversLicense>(StateInvalidMessage);

        number = (number ?? string.Empty).Trim();

        if (validRange is null)
            return Result.Failure<DriversLicense>(DateRangeInvalidMessage);

        if (number.Length < MinimumLength)
            return Result.Failure<DriversLicense>(UnderMinimumLengthMessage);

        if (number.Length > MaximumLength)
            return Result.Failure<DriversLicense>(OverMaximumLengthMessage);

        return Result.Success(
            new DriversLicense(
                number: number,
                state: state,
                validDateRange: validRange));
    }

    public Result<DriversLicense> NewNumber(string newNumber) =>
        Result.Success(
            new DriversLicense(
                number: newNumber,
                state: State,
                validDateRange: ValidDateRange));

    public Result<DriversLicense> NewState(State newState)
    {
        if (Number is null)
            return Result.Failure<DriversLicense>(RequiredMessage);

        return Result.Success(
            new DriversLicense(
                number: Number,
                state: newState,
                validDateRange: ValidDateRange));
    }

    public Result<DriversLicense> NewValidRange(DateTime start, DateTime end)
    {
        if (Number is null)
            return Result.Failure<DriversLicense>(RequiredMessage);

        return Result.Success(
            Create(
                number: Number,
                state: State,
                validRange: DateTimeRange.Create(
                    start: start,
                    end: end).Value)
                .Value);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return State;
        yield return (IComparable)ValidDateRange;

        if (Number is null)
            yield break;

    }

    // EF requires an empty constructor
    protected DriversLicense()
    {
        Number = string.Empty;
        State = State.AL;
        ValidDateRange = DateTimeRange.Create(
            start: DateTime.Today,
            end: DateTime.Today.AddYears(1)).Value;
    }
}
