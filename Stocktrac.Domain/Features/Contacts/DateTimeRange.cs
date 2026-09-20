using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DateTimeRange
{
    public const string EndBeforeStartMessage = "End date must occur after the start date.";

    public const string DateCalculationMessage = "The requested date range is outside the supported range.";

    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateTimeRange(DateOnly start, DateOnly end) =>
        (Start, End) = (start, end);

    public static Result<DateTimeRange> Create(DateOnly start, DateOnly end) =>
        Result.Success((Start: start, End: end))
            .Ensure(range => range.Start < range.End, EndBeforeStartMessage)
            .Map(range => new DateTimeRange(range.Start, range.End));

    public static Result<DateTimeRange> CreateDaysRange(DateOnly start, int days) =>
        CalculateEnd(start, () => start.AddDays(days));

    public static Result<DateTimeRange> CreateWeeksRange(DateOnly start, int weeks) =>
        CalculateEnd(start, () => start.AddDays(checked(7 * weeks)));

    public static Result<DateTimeRange> CreateMonthsRange(DateOnly start, int months) =>
        CalculateEnd(start, () => start.AddMonths(months));

    internal static Result<DateTimeRange> CalculateEnd(
        DateOnly start,
        Func<DateOnly> calculation) =>
        Result.Try(
                calculation,
                _ => DateCalculationMessage)
            .Bind(end => Create(start, end));
}
