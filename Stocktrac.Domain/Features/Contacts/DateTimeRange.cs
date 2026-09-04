using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public class DateTimeRange : ValueObject
{
    public static readonly string RequiredMessage = $"Please include all required items.";
    public static readonly string EndBeforeStartMessage = "End date cannot occur before Start date";
    public static readonly string DateCalculationMessage = "The requested date range is outside the supported range.";

    public DateTime Start { get; private set; } = DateTime.Today;
    public DateTime End { get; private set; } = DateTime.MaxValue;

    private DateTimeRange(DateTime start, DateTime end) =>
        (Start, End) = (start, end);

    public static Result<DateTimeRange> Create(DateTime start, DateTime end) =>
        start >= end
            ? Result.Failure<DateTimeRange>(EndBeforeStartMessage)
            : Result.Success(new DateTimeRange(start, end));

    public static Result<DateTimeRange> Create(DateTime start, TimeSpan duration) =>
        CalculateEnd(start, () => start.Add(duration));

    public Result<int> DurationInMinutes()
    {
        var minutes = (End - Start).TotalMinutes;

        return minutes is < int.MinValue or > int.MaxValue
            ? Result.Failure<int>(DateCalculationMessage)
            : Result.Success((int)minutes);
    }

    public Result<DateTimeRange> NewStart(DateTime newStart) =>
        newStart >= End
            ? Result.Failure<DateTimeRange>(RequiredMessage)
            : Result.Success(
                new DateTimeRange(newStart, End));

    public Result<DateTimeRange> NewEnd(DateTime newEnd) =>
        Start >= newEnd
            ? Result.Failure<DateTimeRange>(RequiredMessage)
            : Result.Success(
                new DateTimeRange(Start, newEnd));

    public Result<DateTimeRange> ClearEnd() =>
        Result.Success(
            new DateTimeRange(Start, DateTime.MaxValue));

    public Result<DateTimeRange> NewDuration(TimeSpan newDuration) =>
        Create(Start, newDuration);

    public static Result<DateTimeRange> CreateDaysRange(DateTime start, int days) =>
        CalculateEnd(start, () => start.AddDays(days));

    public static Result<DateTimeRange> CreateWeeksRange(DateTime start, int weeks) =>
        CalculateEnd(start, () => start.AddDays(7d * weeks));

    public static Result<DateTimeRange> CreateMonthsRange(DateTime start, int months) =>
        CalculateEnd(start, () => start.AddMonths(months));

    private static Result<DateTimeRange> CalculateEnd(DateTime start, Func<DateTime> calculation)
    {
        try
        {
            return Create(start, calculation());
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Failure<DateTimeRange>(DateCalculationMessage);
        }
        catch (OverflowException)
        {
            return Result.Failure<DateTimeRange>(DateCalculationMessage);
        }
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return End;
        yield return Start;
    }

    // EF requires an empty constructor
    protected DateTimeRange() { }
}
