using CSharpFunctionalExtensions;

namespace Stocktrac.Domain.Features.Contacts;

public static class DateRangeExtensions
{
    public static Result<int> DurationInDays(this DateRange range) =>
        range is null
            ? Result.Failure<int>(DateRange.EndBeforeStartMessage)
            : Result.Success(range.End.DayNumber - range.Start.DayNumber);

    public static Result<DateRange> ReplaceStart(this DateRange range, DateOnly newStart) =>
        range is null
            ? Result.Failure<DateRange>(DateRange.EndBeforeStartMessage)
            : DateRange.Create(newStart, range.End);

    public static Result<DateRange> ReplaceEnd(this DateRange range, DateOnly newEnd) =>
        range is null
            ? Result.Failure<DateRange>(DateRange.EndBeforeStartMessage)
            : DateRange.Create(range.Start, newEnd);

    public static Result<DateRange> RemoveEnd(this DateRange range) =>
        range is null
            ? Result.Failure<DateRange>(DateRange.EndBeforeStartMessage)
            : DateRange.Create(range.Start, DateOnly.MaxValue);
}
