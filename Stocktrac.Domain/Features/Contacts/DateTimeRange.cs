using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DateTimeRange
{
    public const string RequiredMessage = "Please include all required items.";
    public const string EndBeforeStartMessage = "End date cannot occur before Start date.";
    public const string DateCalculationMessage = "The requested date range is outside the supported range.";

    public DateTime Start { get; } = DateTime.Today;
    public DateTime End { get; } = DateTime.MaxValue;

    private DateTimeRange(DateTime start, DateTime end) =>
        (Start, End) = (start, end);

    public static Result<DateTimeRange> Create(DateTime start, DateTime end) =>
        Result.Success((Start: start, End: end))
            .Ensure(range => range.Start < range.End, EndBeforeStartMessage)
            .Map(range => new DateTimeRange(range.Start, range.End));

    public static Result<DateTimeRange> Create(DateTime start, TimeSpan duration) =>
        CalculateEnd(start, () => start.Add(duration));

    public Result<int> DurationInMinutes()
        => Result.Success((End - Start).TotalMinutes)
            .Ensure(minutes => minutes is >= int.MinValue and <= int.MaxValue, DateCalculationMessage)
            .Map(minutes => (int)minutes);

    public Result<DateTimeRange> WithStart(DateTime newStart) =>
        Result.Success(newStart)
            .Ensure(start => start < End, RequiredMessage)
            .Bind(start => Create(start, End));

    public Result<DateTimeRange> WithEnd(DateTime newEnd) =>
        Result.Success(newEnd)
            .Ensure(end => Start < end, RequiredMessage)
            .Bind(end => Create(Start, end));

    public Result<DateTimeRange> WithoutEnd() =>
        Result.Success(new DateTimeRange(Start, DateTime.MaxValue));

    public Result<DateTimeRange> WithDuration(TimeSpan newDuration) =>
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
}
