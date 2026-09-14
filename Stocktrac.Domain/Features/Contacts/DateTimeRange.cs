using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DateTimeRange
{
    public const string RequiredMessage = "Please include all required items.";
    public const string EndBeforeStartMessage = "End date cannot occur before Start date.";
    public const string DateCalculationMessage = "The requested date range is outside the supported range.";

    public DateTime Start { get; private set; } = DateTime.Today;
    public DateTime End { get; private set; } = DateTime.MaxValue;

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

    private static Result<DateTimeRange> CalculateEnd(
        DateTime start,
        Func<DateTime> calculation) =>
        Result.Try(
                calculation,
                _ => DateCalculationMessage)
            .Bind(end => Create(start, end));

    internal DateTimeRange WithStartValue(DateTime start) =>
        this with { Start = start };

    internal DateTimeRange WithEndValue(DateTime end) =>
        this with { End = end };
}
