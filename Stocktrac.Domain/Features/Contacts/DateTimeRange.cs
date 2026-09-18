using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DateTimeRange
{
    public const string RequiredMessage = "Please include all required items.";
    public const string EndBeforeStartMessage = "End date must occur after start date.";
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
        DateTimeRangeExtensions.CalculateEnd(start, () => start.Add(duration));

    public static Result<DateTimeRange> CreateDaysRange(DateTime start, int days) =>
        DateTimeRangeExtensions.CalculateEnd(start, () => start.AddDays(days));

    public static Result<DateTimeRange> CreateWeeksRange(DateTime start, int weeks) =>
        DateTimeRangeExtensions.CalculateEnd(start, () => start.AddDays(7d * weeks));

    public static Result<DateTimeRange> CreateMonthsRange(DateTime start, int months) =>
        DateTimeRangeExtensions.CalculateEnd(start, () => start.AddMonths(months));
}
