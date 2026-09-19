using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DateTimeRange
{
    public const string EndBeforeStartMessage = "End date must occur after the start date.";

    public const string DateCalculationMessage = "The requested date range is outside the supported range.";

    public DateTime Start { get; }
    public DateTime End { get; }

    private DateTimeRange(DateTime start, DateTime end) =>
        (Start, End) = (start, end);

    public static Result<DateTimeRange> Create(DateTime start, DateTime end) =>
        Result.Success((Start: start, End: end))
            .Ensure(range => range.Start < range.End, EndBeforeStartMessage)
            .Map(range => new DateTimeRange(range.Start, range.End));

    public static Result<DateTimeRange> Create(DateTime start, TimeSpan duration) =>
        CalculateEnd(start, () => start.Add(duration));

    public static Result<DateTimeRange> CreateDaysRange(DateTime start, int days) =>
        CalculateEnd(start, () => start.AddDays(days));

    public static Result<DateTimeRange> CreateWeeksRange(DateTime start, int weeks) =>
        CalculateEnd(start, () => start.AddDays(7d * weeks));

    public static Result<DateTimeRange> CreateMonthsRange(DateTime start, int months) =>
        CalculateEnd(start, () => start.AddMonths(months));

    private static Result<DateTimeRange> CalculateEnd(DateTime start, Func<DateTime> calculateEnd)
    {
        try
        {
            return Create(start, calculateEnd());
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Failure<DateTimeRange>(DateCalculationMessage);
        }
    }
}