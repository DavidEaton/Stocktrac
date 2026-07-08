using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contact;

public class DateTimeRange : ValueObject
{
    public static readonly string RequiredMessage = $"Please include all required items.";
    public static readonly string EndBeforeStartMessage = "End date cannot occur before Start date";

    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    private DateTimeRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public static Result<DateTimeRange> Create(DateTime start, DateTime end) =>
        start >= end
            ? Result.Failure<DateTimeRange>(EndBeforeStartMessage)
            : Result.Success(new DateTimeRange(start, end));

    public DateTimeRange(DateTime start, TimeSpan duration) : this(start, start.Add(duration)) { }

    public Result<int> DurationInMinutes() =>
        Result.Success((End - Start).Minutes);

    public Result<DateTimeRange> NewEnd(DateTime newEnd) =>
        Start >= newEnd
            ? Result.Failure<DateTimeRange>(RequiredMessage)
            : Result.Success(new DateTimeRange(Start, newEnd));

    public Result<DateTimeRange> NewDuration(TimeSpan newDuration) =>
        Result.Success(new DateTimeRange(Start, newDuration));

    public Result<DateTimeRange> NewStart(DateTime newStart) =>
        newStart >= End
            ? Result.Failure<DateTimeRange>(RequiredMessage)
            : Result.Success(new DateTimeRange(newStart, End));

    public static Result<DateTimeRange> CreateOneDayRange(DateTime start) =>
        Result.Success(new DateTimeRange(start, start.AddDays(1)));

    public static Result<DateTimeRange> CreateOneWeekRange(DateTime start) =>
        Result.Success(new DateTimeRange(start, start.AddDays(7)));

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return End;
        yield return Start;
    }

    // EF requires an empty constructor
    protected DateTimeRange() { }
}
