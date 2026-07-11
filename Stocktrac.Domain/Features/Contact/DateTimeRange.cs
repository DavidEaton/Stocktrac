using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contact;

public class DateTimeRange : ValueObject
{
    public static readonly string RequiredMessage = $"Please include all required items.";
    public static readonly string EndBeforeStartMessage = "End date cannot occur before Start date";

    public DateTime Start { get; private set; } = DateTime.Today;
    public DateTime End { get; private set; } = DateTime.MaxValue;

    private DateTimeRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public static Result<DateTimeRange> Create(DateTime start, DateTime end) =>
        start >= end
            ? Result.Failure<DateTimeRange>(EndBeforeStartMessage)
            : Result.Success(new DateTimeRange(start, end));

    public DateTimeRange(DateTime start, TimeSpan duration)
        : this(start, start.Add(duration)) { }

    public Result<int> DurationInMinutes() =>
        Result.Success((End - Start).Minutes);

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
        Result.Success(
            new DateTimeRange(Start, newDuration));

    public static Result<DateTimeRange> CreateDaysRange(DateTime start, int days) =>
        Result.Success(
            new DateTimeRange(start, start.AddDays(days)));

    public static Result<DateTimeRange> CreateWeeksRange(DateTime start, int weeks) =>
        Result.Success(
            new DateTimeRange(start, start.AddDays(7 * weeks)));

    public static Result<DateTimeRange> CreateMonthsRange(DateTime start, int months) =>
        Result.Success(
            new DateTimeRange(start, start.AddMonths(months)));

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return End;
        yield return Start;
    }

    // EF requires an empty constructor
    protected DateTimeRange() { }
}
