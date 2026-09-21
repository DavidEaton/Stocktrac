using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public sealed record DateRange
{
    public const string EndBeforeStartMessage = "End date must occur after the start date.";

    public const string DateCalculationMessage = "The requested date range is outside the supported range.";

    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRange(DateOnly start, DateOnly end) =>
        (Start, End) = (start, end);

    public static Result<DateRange> Create(DateOnly start, DateOnly end) =>
        Result.Success((Start: start, End: end))
            .Ensure(range => range.Start < range.End, EndBeforeStartMessage)
            .Map(range => new DateRange(range.Start, range.End));

    public static Result<DateRange> CreateDaysRange(DateOnly start, int days) =>
        CalculateEnd(start, () => start.AddDays(days));

    public static Result<DateRange> CreateWeeksRange(DateOnly start, int weeks) =>
        CalculateEnd(start, () => start.AddDays(checked(7 * weeks)));

    public static Result<DateRange> CreateMonthsRange(DateOnly start, int months) =>
        CalculateEnd(start, () => start.AddMonths(months));

    internal static Result<DateRange> CalculateEnd(
        DateOnly start,
        Func<DateOnly> calculation) =>
        Result.Try(
                calculation,
                _ => DateCalculationMessage)
            .Bind(end => Create(start, end));
}
